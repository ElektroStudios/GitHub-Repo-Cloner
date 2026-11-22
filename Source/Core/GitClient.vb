Option Strict On
Option Explicit On
Option Infer Off

Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.Win32.SafeHandles

''' <summary>Runs Git without a console window and reports transfer information from stderr.</summary>
Public NotInheritable Class GitClient
    Implements IGitClient

    Private Shared ReadOnly StalledNetwork As New Regex(
        "could not resolve (host|proxy)|failed to connect|connection (reset|timed out|closed|refused)|operation (timed out|too slow)|early EOF|remote end hung up|HTTP (429|50[0234])|returned error: (429|50[0234])|SSL connection timeout|unexpected disconnect",
        RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private gitPath As String

    Public Async Function CheckAsync(cancellation As CancellationToken) As Task Implements IGitClient.CheckAsync
        cancellation.ThrowIfCancellationRequested()
        gitPath = FindGit()
        Dim result As CommandResult = Await RunAsync("--version", Sub(transfer As GitTransfer) Return, cancellation).ConfigureAwait(False)
        If result.ExitCode <> 0 OrElse Not result.Output.Any(Function(line As String) line.StartsWith("git version ", StringComparison.OrdinalIgnoreCase)) Then
            Throw New InvalidOperationException("Git could not be started correctly. Verify your Git for Windows installation.")
        End If
    End Function

    Public Async Function CloneAsync(repositoryUri As Uri, target As String, report As Action(Of GitTransfer), cancellation As CancellationToken) As Task Implements IGitClient.CloneAsync
        If repositoryUri Is Nothing Then Throw New ArgumentNullException(NameOf(repositoryUri))
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        If report Is Nothing Then Throw New ArgumentNullException(NameOf(report))
        If gitPath Is Nothing Then Throw New InvalidOperationException("Verify Git before cloning a repository.")

        Dim arguments As String = $"-c color.ui=false -c http.lowSpeedLimit=1 -c http.lowSpeedTime=60 clone --progress -- {QuoteArgument(repositoryUri.AbsoluteUri)} {QuoteArgument(target)}"
        Dim result As CommandResult = Await RunAsync(arguments, report, cancellation).ConfigureAwait(False)
        If result.ExitCode <> 0 Then
            Dim details As String = If(result.Output.Length = 0, "Git returned no details.", String.Join(" ", result.Output))
            Throw New GitCommandException($"Git exit {result.ExitCode}: {details}", StalledNetwork.IsMatch(details))
        End If
    End Function

    Private Async Function RunAsync(arguments As String, report As Action(Of GitTransfer), cancellation As CancellationToken) As Task(Of CommandResult)
        cancellation.ThrowIfCancellationRequested()
        Dim start As New ProcessStartInfo(gitPath, arguments) With {
            .UseShellExecute = False, .RedirectStandardOutput = True, .RedirectStandardError = True,
            .RedirectStandardInput = True, .CreateNoWindow = True
        }
        start.EnvironmentVariables("GIT_TERMINAL_PROMPT") = "0"
        start.EnvironmentVariables("GCM_INTERACTIVE") = "never"
        start.EnvironmentVariables("LANG") = "C"
        start.EnvironmentVariables("LC_ALL") = "C"
        start.EnvironmentVariables.Remove("GITHUB_TOKEN")

        Using process As New Process With {.StartInfo = start, .EnableRaisingEvents = True}
            Dim exited As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
            AddHandler process.Exited, Sub(sender As Object, args As EventArgs) exited.TrySetResult(True)
            Try
                If Not process.Start() Then Throw New InvalidOperationException("Git could not be started.")
            Catch ex As Win32Exception
                Throw New IOException($"Could not launch Git at '{gitPath}': {ex.Message}", ex)
            End Try
            Dim job As SafeFileHandle = Nothing
#If NETFRAMEWORK Then
            job = WindowsProcessJob.TryAttach(process)
#End If
            Using job
                Return Await ReadProcessAsync(process, exited, report, job, cancellation).ConfigureAwait(False)
            End Using
        End Using
    End Function

    Private Shared Async Function ReadProcessAsync(process As Process, exited As TaskCompletionSource(Of Boolean),
                                                   report As Action(Of GitTransfer), job As SafeFileHandle,
                                                   cancellation As CancellationToken) As Task(Of CommandResult)
        process.StandardInput.Close()
        If process.HasExited Then exited.TrySetResult(True)
        Dim output As Task(Of String()) = ReadOutputAsync(process.StandardOutput, Sub(transfer As GitTransfer) Return)
        Dim errors As Task(Of String()) = ReadOutputAsync(process.StandardError, report)
        Dim finished As Task = Task.WhenAll(exited.Task, output, errors)
        Dim canceled As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)

        Using registration As CancellationTokenRegistration = cancellation.Register(Sub() canceled.TrySetResult(True))
            If Await Task.WhenAny(finished, canceled.Task).ConfigureAwait(False) Is canceled.Task Then
                If job IsNot Nothing Then job.Dispose()
                If job Is Nothing OrElse Not process.WaitForExit(1500) Then StopProcess(process)
                Try
                    Await finished.ConfigureAwait(False)
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ObjectDisposedException
                    cancellation.ThrowIfCancellationRequested()
                End Try
                cancellation.ThrowIfCancellationRequested()
            End If
        End Using

        Await finished.ConfigureAwait(False)
        cancellation.ThrowIfCancellationRequested()
        Return New CommandResult(process.ExitCode, output.Result.Concat(errors.Result).ToArray())
    End Function

    Private Shared Async Function ReadOutputAsync(reader As StreamReader, report As Action(Of GitTransfer)) As Task(Of String())
        Dim tail As New Queue(Of String)()
        Dim last As GitTransfer = Nothing
        Dim timer As Stopwatch = Stopwatch.StartNew()

        While True
            Dim line As String = Await reader.ReadLineAsync().ConfigureAwait(False)
            If line Is Nothing Then Exit While
            Dim transfer As GitTransfer = GitProgressParser.Parse(line)
            If transfer IsNot Nothing Then
                If GitProgressParser.ShouldReport(last, transfer, timer.ElapsedMilliseconds) Then
                    report(transfer)
                    last = transfer
                    timer.Restart()
                End If
            ElseIf Not String.IsNullOrWhiteSpace(line) Then
                tail.Enqueue(If(line.Length > 400, line.Substring(0, 400), line))
                If tail.Count > 8 Then tail.Dequeue()
            End If
        End While
        Return tail.ToArray()
    End Function

    Private Shared Sub StopProcess(process As Process)
        If process.HasExited Then Return
#If NETFRAMEWORK Then
        Dim command As New ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "taskkill.exe"),
            FormattableString.Invariant($"/PID {process.Id} /T /F")) With {.UseShellExecute = False, .CreateNoWindow = True}
        Try
            Using killer As Process = Process.Start(command)
                If Not killer.WaitForExit(5000) Then
                    killer.Kill()
                End If
            End Using
        Catch ex As Win32Exception
            If Not process.HasExited Then process.Kill()
        Catch ex As InvalidOperationException When process.HasExited
            Return
        End Try
        If Not process.WaitForExit(1500) Then process.Kill()
#Else
        process.Kill(entireProcessTree:=True)
#End If
        If Not process.WaitForExit(5000) Then Throw New IOException("Git did not exit after it was asked to stop.")
    End Sub

    Private Shared Function FindGit() As String
        Dim windows As Boolean = Path.DirectorySeparatorChar = ChrW(92)
        Dim executable As String = If(windows, "git.exe", "git")
        Dim entries As New List(Of String)(If(Environment.GetEnvironmentVariable("PATH"), String.Empty).Split(Path.PathSeparator))
        If windows Then
            For Each folder As Environment.SpecialFolder In {Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolder.LocalApplicationData}
                Dim root As String = Environment.GetFolderPath(folder)
                If root.Length > 0 Then
                    entries.Add(If(folder = Environment.SpecialFolder.LocalApplicationData, Path.Combine(root, "Programs", "Git", "cmd"), Path.Combine(root, "Git", "cmd")))
                End If
            Next
        End If
        For Each entry As String In entries
            Dim directory As String = entry.Trim().Trim(""""c)
            If directory.IndexOfAny(Path.GetInvalidPathChars()) >= 0 OrElse Not Path.IsPathRooted(directory) Then Continue For
            Dim candidate As String = Path.Combine(directory, executable)
            If File.Exists(candidate) Then Return candidate
        Next
        Throw New FileNotFoundException("Git was not found. Install Git for Windows or add its cmd folder to PATH, then restart the app.")
    End Function

    Friend Shared Function QuoteArgument(value As String) As String
        If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
        Dim result As New StringBuilder()
        Dim backslash As Char = ChrW(92)
        result.Append(""""c)
        Dim slashes As Integer = 0
        For Each character As Char In value
            If character = backslash Then
                slashes += 1
            Else
                result.Append(backslash, If(character = """"c, (slashes * 2) + 1, slashes))
                result.Append(character)
                slashes = 0
            End If
        Next
        result.Append(backslash, slashes * 2).Append(""""c)
        Return result.ToString()
    End Function

    Private NotInheritable Class CommandResult
        Public ReadOnly ExitCode As Integer
        Public ReadOnly Output As String()
        Public Sub New(exitCode As Integer, output As String())
            Me.ExitCode = exitCode
            Me.Output = output
        End Sub
    End Class

End Class
