Option Strict On
Option Explicit On
Option Infer Off

Imports System.ComponentModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>Coordinates discovery, staged clones, retries, and the cooldown between repositories.</summary>
Public NotInheritable Class CloneBatch

    Private Shared ReadOnly RepositoryPattern As New Regex("\A[A-Za-z0-9_.-]{1,100}\z", RegexOptions.CultureInvariant)
    Private Shared ReadOnly WindowsDevice As New Regex("\A(?:CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])(?:\.|\z)", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private ReadOnly catalog As IRepositoryCatalog
    Private ReadOnly git As IGitClient
    Private ReadOnly pause As Func(Of TimeSpan, CancellationToken, Task)

    Public Sub New(catalog As IRepositoryCatalog, git As IGitClient)
        Me.New(catalog, git, AddressOf Task.Delay)
    End Sub

    Friend Sub New(catalog As IRepositoryCatalog, git As IGitClient, pause As Func(Of TimeSpan, CancellationToken, Task))
        If catalog Is Nothing Then Throw New ArgumentNullException(NameOf(catalog))
        If git Is Nothing Then Throw New ArgumentNullException(NameOf(git))
        If pause Is Nothing Then Throw New ArgumentNullException(NameOf(pause))
        Me.catalog = catalog
        Me.git = git
        Me.pause = pause
    End Sub

    Public Async Function RunAsync(options As CloneOptions, progress As IProgress(Of BatchUpdate), cancellation As CancellationToken) As Task(Of BatchUpdate)
        If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
        If progress Is Nothing Then Throw New ArgumentNullException(NameOf(progress))
        cancellation.ThrowIfCancellationRequested()

        Dim state As New BatchState(progress)
        state.Publish("Checking Git…")
        Await git.CheckAsync(cancellation).ConfigureAwait(False)
        Await Task.Run(Sub() Directory.CreateDirectory(options.DirectoryPath), cancellation).ConfigureAwait(False)
        Dim repositories As IReadOnlyList(Of Repository) =
            Await catalog.GetRepositoriesAsync(options.Owner, Sub(message As String) state.Publish(message), cancellation).ConfigureAwait(False)
        cancellation.ThrowIfCancellationRequested()

        Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim selected As New List(Of Repository)()
        For Each repository As Repository In repositories
            If (options.IncludeForks OrElse Not repository.IsFork) AndAlso names.Add(repository.Name) Then selected.Add(repository)
        Next
        state.Total = selected.Count
        state.Publish(If(state.Total = 0, "No matching public repositories found.", $"Found {state.Total} matching public repositories."))

        For Each repository As Repository In selected
            cancellation.ThrowIfCancellationRequested()
            state.Current = repository.Name
            If Not IsSafeName(repository.Name) Then
                state.Failed += 1
                state.Publish($"Skipped unsafe Windows repository name: {repository.Name}.", BatchMessageKind.Failure)
            Else
                Await CloneOneAsync(options, repository, state, cancellation).ConfigureAwait(False)
            End If

            state.Current = Nothing
        Next

        cancellation.ThrowIfCancellationRequested()
        Return state.Publish(If(state.Failed = 0, "Batch finished.", "Batch finished with errors."),
                             If(state.Failed = 0, BatchMessageKind.Success, BatchMessageKind.Warning))
    End Function

    Friend Shared Function IsSafeName(name As String) As Boolean
        Return name IsNot Nothing AndAlso RepositoryPattern.IsMatch(name) AndAlso Not name.EndsWith(".", StringComparison.Ordinal) AndAlso
               Not WindowsDevice.IsMatch(name) AndAlso Not String.Equals(name, ".git", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Async Function CloneOneAsync(options As CloneOptions, repository As Repository, state As BatchState, cancellation As CancellationToken) As Task
        Dim destination As String = Path.Combine(options.DirectoryPath, repository.Name)
        If SkipExisting(destination, state) Then Return
        Dim uri As New Uri($"https://github.com/{options.Owner}/{repository.Name}.git")

        For attempt As Integer = 1 To 3
            cancellation.ThrowIfCancellationRequested()
            Dim staging As String = Path.Combine(options.DirectoryPath, $".cloner-{Guid.NewGuid():N}")
            Dim failure As Exception = Nothing
            Dim retry As Boolean = False
            Dim receivedSize As String = Nothing
            Try
                cancellation.ThrowIfCancellationRequested()
                Directory.CreateDirectory(staging)
                If attempt = 1 AndAlso state.CooldownPending Then
                    state.Publish($"Waiting {options.Delay.TotalSeconds:g} s before cloning {repository.Name}…")
                    Await pause(options.Delay, cancellation).ConfigureAwait(False)
                    state.CooldownPending = False
                    cancellation.ThrowIfCancellationRequested()
                    If SkipExisting(destination, state) Then Return
                End If
                state.Publish($"Cloning {repository.Name} (attempt {attempt}/3)…")
                state.CooldownPending = options.Delay > TimeSpan.Zero
                Await git.CloneAsync(uri, staging,
                    Sub(transfer As GitTransfer)
                        If transfer.Size.Length > 0 Then receivedSize = transfer.Size
                        state.Publish(Nothing, transfer:=transfer)
                    End Sub, cancellation).ConfigureAwait(False)
                cancellation.ThrowIfCancellationRequested()
                If Not Directory.Exists(Path.Combine(staging, ".git")) Then
                    Throw New IOException("Git reported success but did not produce a working copy.")
                End If

                Try
                    Directory.Move(staging, destination)
                Catch ex As IOException When Exists(destination)
                    state.Skipped += 1
                    state.Publish($"Destination appeared while cloning: {repository.Name}.", BatchMessageKind.Warning)
                    Return
                End Try

                state.Cloned += 1
                state.Publish(If(receivedSize Is Nothing, $"Cloned {repository.Name}.", $"Cloned {repository.Name} · received {receivedSize}."),
                              BatchMessageKind.Success)
                Return
            Catch ex As GitCommandException
                failure = ex
                retry = ex.IsTransient AndAlso attempt < 3
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                             TypeOf ex Is Win32Exception OrElse TypeOf ex Is NotSupportedException
                failure = ex
            Finally
                If Directory.Exists(staging) Then
                    Try
                        DeleteStaging(staging)
                    Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                        state.Publish($"Could not remove temporary folder '{staging}': {ex.Message}", BatchMessageKind.Warning)
                    End Try
                End If
            End Try

            If Not retry Then
                state.Failed += 1
                state.Publish($"Could not clone {repository.Name}: {failure.Message}", BatchMessageKind.Failure)
                Return
            End If

            Dim delay As TimeSpan = TimeSpan.FromSeconds(Math.Pow(2, attempt))
            state.Publish($"{repository.Name}: {failure.Message} Retrying in {delay.TotalSeconds:g} s…", BatchMessageKind.Warning)
            Await pause(delay, cancellation).ConfigureAwait(False)
        Next
    End Function

    Private Shared Sub DeleteStaging(path As String)
        Dim pending As New Stack(Of DirectoryInfo)()
        Dim directories As New Stack(Of DirectoryInfo)()
        pending.Push(New DirectoryInfo(path))
        While pending.Count > 0
            Dim directory As DirectoryInfo = pending.Pop()
            If (directory.Attributes And FileAttributes.ReparsePoint) <> 0 Then
                directory.Delete()
                Continue While
            End If

            directories.Push(directory)
            For Each entry As FileSystemInfo In directory.EnumerateFileSystemInfos()
                Dim child As DirectoryInfo = TryCast(entry, DirectoryInfo)
                If child IsNot Nothing Then
                    pending.Push(child)
                Else
                    Dim attributes As FileAttributes = entry.Attributes
                    If (attributes And (FileAttributes.ReadOnly Or FileAttributes.ReparsePoint)) = FileAttributes.ReadOnly Then
                        entry.Attributes = attributes And Not FileAttributes.ReadOnly
                    End If
                    entry.Delete()
                End If
            Next
        End While

        While directories.Count > 0
            directories.Pop().Delete()
        End While
    End Sub

    Private Shared Function SkipExisting(destination As String, state As BatchState) As Boolean
        If Not Exists(destination) Then Return False
        state.Skipped += 1
        state.Publish($"Already exists: {state.Current}.", BatchMessageKind.Warning)
        Return True
    End Function

    Private Shared Function Exists(path As String) As Boolean
        Return Directory.Exists(path) OrElse File.Exists(path)
    End Function

    Private NotInheritable Class BatchState
        Private ReadOnly progress As IProgress(Of BatchUpdate)
        Public CooldownPending As Boolean
        Public Total As Integer
        Public Cloned As Integer
        Public Skipped As Integer
        Public Failed As Integer
        Public Current As String

        Public Sub New(progress As IProgress(Of BatchUpdate))
            Me.progress = progress
        End Sub

        Public Function Publish(message As String, Optional kind As BatchMessageKind = BatchMessageKind.Information,
                                Optional transfer As GitTransfer = Nothing) As BatchUpdate
            Dim update As New BatchUpdate(Total, Cloned, Skipped, Failed, Current, transfer, message, kind)
            progress.Report(update)
            Return update
        End Function
    End Class
End Class
