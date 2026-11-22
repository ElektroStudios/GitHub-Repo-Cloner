Option Strict On
Option Explicit On
Option Infer Off

Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports GitHub_Repo_Cloner.Core

Public NotInheritable Class BatchTests

    <Fact>
    Public Async Function CountsOutcomesAndPreservesExistingDestinations() As Task
        Using temp As New TemporaryFolder()
            Dim existing As String = Path.Combine(temp.Value, "Existing")
            File.WriteAllText(existing, "keep")
            Dim successPaths As New List(Of String)()
            Dim git As New FakeGit(
                Function(uri As Uri, staging As String, cancellation As CancellationToken) As Task
                    If uri.AbsolutePath.EndsWith("/Broken.git", StringComparison.Ordinal) Then
                        Dim partialFile As String = Path.Combine(staging, "partial.txt")
                        File.WriteAllText(partialFile, "partial")
                        File.SetAttributes(partialFile, File.GetAttributes(partialFile) Or FileAttributes.ReadOnly)
                        Throw New GitCommandException("permanent", False)
                    End If
                    successPaths.Add(uri.AbsolutePath)
                    Directory.CreateDirectory(Path.Combine(staging, ".git"))
                    Return Task.CompletedTask
                End Function)
            Dim waits As New List(Of TimeSpan)()
            Dim batch As CloneBatch = MakeBatch(git, waits, {"Existing", "Broken", "Healthy", "healthy", "CON", "../external"}, includeFork:=True)
            Dim progress As New Recorder()
            Dim result As BatchUpdate = Await batch.RunAsync(Options(temp, 3), progress, CancellationToken.None).ConfigureAwait(False)
            Assert.Equal(5, result.Total)
            Assert.Equal(5, result.Processed)
            Assert.Equal(1, result.Cloned)
            Assert.Equal(1, result.Skipped)
            Assert.Equal(3, result.Failed)
            Assert.Equal("keep", File.ReadAllText(existing))
            Assert.True(Directory.Exists(Path.Combine(temp.Value, "Healthy", ".git")))
            Assert.False(Directory.Exists(Path.Combine(temp.Value, "Broken")))
            Assert.Equal("/owner/Healthy.git", Assert.Single(successPaths))
            Assert.Single(waits)
            Assert.All(waits, Sub(wait As TimeSpan) Assert.Equal(TimeSpan.FromSeconds(3), wait))
            Assert.Empty(Directory.GetDirectories(temp.Value, ".cloner-*"))
        End Using
    End Function

    <Fact>
    Public Async Function RetriesTransientGitFailuresInFreshFolders() As Task
        Using temp As New TemporaryFolder()
            Dim seen As New List(Of String)()
            Dim git As New FakeGit(
                Function(uri As Uri, staging As String, cancellation As CancellationToken) As Task
                    Assert.Empty(Directory.GetFileSystemEntries(staging))
                    seen.Add(staging)
                    If seen.Count < 3 Then
                        File.WriteAllText(Path.Combine(staging, "partial"), "data")
                        Throw New GitCommandException("early EOF", True)
                    End If
                    Directory.CreateDirectory(Path.Combine(staging, ".git"))
                    Return Task.CompletedTask
                End Function)
            Dim waits As New List(Of TimeSpan)()
            Dim result As BatchUpdate = Await MakeBatch(git, waits, {"Repo"}).RunAsync(Options(temp), New Recorder(), CancellationToken.None).ConfigureAwait(False)
            Assert.Equal(3, seen.Distinct().Count())
            Assert.Equal(1, result.Cloned)
            Assert.Collection(waits, Sub(wait As TimeSpan) Assert.Equal(TimeSpan.FromSeconds(2), wait),
                                      Sub(wait As TimeSpan) Assert.Equal(TimeSpan.FromSeconds(4), wait))
            Assert.Empty(Directory.GetDirectories(temp.Value, ".cloner-*"))
        End Using
    End Function

    <Fact>
    Public Async Function CancelingCloneKeepsEarlierReposAndCleansPartialClone() As Task
        Using temp As New TemporaryFolder(), cancellation As New CancellationTokenSource()
            Dim git As New FakeGit(
                Function(uri As Uri, staging As String, token As CancellationToken) As Task
                    Directory.CreateDirectory(Path.Combine(staging, ".git"))
                    If uri.AbsolutePath.EndsWith("/Second.git", StringComparison.Ordinal) Then
                        cancellation.Cancel()
                        Return Task.FromCanceled(token)
                    End If
                    Return Task.CompletedTask
                End Function)
            Dim batch As CloneBatch = MakeBatch(git, New List(Of TimeSpan)(), {"First", "Second", "Never"})
            Await Assert.ThrowsAnyAsync(Of OperationCanceledException)(
                Function() batch.RunAsync(Options(temp), New Recorder(), cancellation.Token)).ConfigureAwait(False)
            Assert.True(Directory.Exists(Path.Combine(temp.Value, "First", ".git")))
            Assert.False(Directory.Exists(Path.Combine(temp.Value, "Second")))
            Assert.False(Directory.Exists(Path.Combine(temp.Value, "Never")))
            Assert.Empty(Directory.GetDirectories(temp.Value, ".cloner-*"))
        End Using
    End Function

    <Fact>
    Public Async Function NeverOverwritesDestinationCreatedMidDownload() As Task
        Using temp As New TemporaryFolder()
            Dim final As String = Path.Combine(temp.Value, "Repo")
            Dim git As New FakeGit(
                Function(uri As Uri, staging As String, token As CancellationToken) As Task
                    Directory.CreateDirectory(Path.Combine(staging, ".git"))
                    Directory.CreateDirectory(final)
                    File.WriteAllText(Path.Combine(final, "precious"), "keep")
                    Return Task.CompletedTask
                End Function)
            Dim batch As CloneBatch = MakeBatch(git, New List(Of TimeSpan)(), {"Repo"})
            Dim result As BatchUpdate = Await batch.RunAsync(Options(temp), New Recorder(), CancellationToken.None).ConfigureAwait(False)
            Assert.Equal(1, result.Skipped)
            Assert.Equal("keep", File.ReadAllText(Path.Combine(final, "precious")))
            Assert.False(Directory.Exists(Path.Combine(final, ".git")))
            Assert.Empty(Directory.GetDirectories(temp.Value, ".cloner-*"))
        End Using
    End Function

    <Fact>
    Public Async Function GitSuccessWithoutWorkingCopyIsReportedAsFailure() As Task
        Using temp As New TemporaryFolder()
            Dim git As New FakeGit(Function(uri As Uri, staging As String, token As CancellationToken) Task.CompletedTask)
            Dim result As BatchUpdate = Await MakeBatch(git, New List(Of TimeSpan)(), {"Repo"}).RunAsync(Options(temp), New Recorder(), CancellationToken.None).ConfigureAwait(False)
            Assert.Equal(1, result.Failed)
            Assert.Empty(Directory.GetFileSystemEntries(temp.Value))
        End Using
    End Function

    <Fact>
    Public Async Function AUserWithOnlyForksHasNoWork() As Task
        Using temp As New TemporaryFolder()
            Dim git As New FakeGit(
                Function(uri As Uri, staging As String, token As CancellationToken) As Task
                    Throw New InvalidOperationException("The fork must not be cloned.")
                End Function)
            Dim result As BatchUpdate = Await MakeBatch(git, New List(Of TimeSpan)(), Array.Empty(Of String)(), includeFork:=True).
                RunAsync(Options(temp), New Recorder(), CancellationToken.None).ConfigureAwait(False)
            Assert.Equal(0, result.Total)
            Assert.Equal(0, result.Processed)
        End Using
    End Function

    <Fact>
    Public Async Function EnabledForkIsClonedAndCounted() As Task
        Using temp As New TemporaryFolder()
            Dim git As New FakeGit(Function(uri As Uri, staging As String, token As CancellationToken) As Task
                                       Directory.CreateDirectory(Path.Combine(staging, ".git"))
                                       Return Task.CompletedTask
                                   End Function)
            Dim batch As CloneBatch = MakeBatch(git, New List(Of TimeSpan)(), {"Original"}, includeFork:=True)
            Dim result As BatchUpdate = Await batch.RunAsync(New CloneOptions("owner", temp.Value, TimeSpan.Zero, True), New Recorder(), CancellationToken.None).ConfigureAwait(False)
            Assert.Equal(2, result.Total)
            Assert.Equal(2, result.Cloned)
            Assert.True(Directory.Exists(Path.Combine(temp.Value, "Fork", ".git")))
        End Using
    End Function

    <Theory>
    <InlineData(0, 0)>
    <InlineData(1, 0)>
    <InlineData(2, 1)>
    Public Async Function ExistingFoldersNeverTriggerCooldown(downloads As Integer, expectedWaits As Integer) As Task
        Using temp As New TemporaryFolder()
            Dim waits As New List(Of TimeSpan)()
            Dim names As New List(Of String)()
            For index As Integer = 0 To downloads
                Dim existing As String = $"Existing-{index}"
                Directory.CreateDirectory(Path.Combine(temp.Value, existing))
                names.Add(existing)
                If index < downloads Then names.Add($"Download-{index}")
            Next
            Dim calls As Integer = 0
            Dim git As New FakeGit(Function(uri As Uri, staging As String, token As CancellationToken) As Task
                                       Assert.Equal($"/owner/Download-{calls}.git", uri.AbsolutePath)
                                       Assert.Equal(calls, waits.Count)
                                       calls += 1
                                       Directory.CreateDirectory(Path.Combine(staging, ".git"))
                                       Return Task.CompletedTask
                                   End Function)
            Dim batch As CloneBatch = MakeBatch(git, waits, names.ToArray())
            Dim result As BatchUpdate = Await batch.RunAsync(Options(temp, 2), New Recorder(), CancellationToken.None).ConfigureAwait(False)
            Assert.Equal(expectedWaits, waits.Count)
            Assert.Equal(downloads + 1, result.Skipped)
            Assert.Equal(downloads, result.Cloned)
        End Using
    End Function

    <Fact>
    Public Async Function CancellationWaitsForGitAndRemovesStagingBeforeBatchFinishes() As Task
        Using temp As New TemporaryFolder(), stopping As New CancellationTokenSource()
            Dim started As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
            Dim release As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
            Dim git As New FakeGit(Async Function(uri As Uri, stage As String, token As CancellationToken) As Task
                                       File.WriteAllText(Path.Combine(stage, "partial"), "data")
                                       started.SetResult(True)
                                       Await release.Task.ConfigureAwait(False)
                                       token.ThrowIfCancellationRequested()
                                   End Function)
            Dim batch As CloneBatch = MakeBatch(git, New List(Of TimeSpan)(), {"Example"})
            Dim running As Task(Of BatchUpdate) = batch.RunAsync(Options(temp), New Recorder(), stopping.Token)
            Await started.Task.ConfigureAwait(False)
            stopping.Cancel()
            Try
                Assert.False(running.IsCompleted)
                Assert.Single(Directory.GetDirectories(temp.Value, ".cloner-*"))
            Finally
                release.TrySetResult(True)
            End Try
            Await Assert.ThrowsAnyAsync(Of OperationCanceledException)(Function() running).ConfigureAwait(False)
            Assert.Empty(Directory.GetFileSystemEntries(temp.Value))
        End Using
    End Function

    Private Shared Function Options(temp As TemporaryFolder, Optional seconds As Integer = 0) As CloneOptions
        Return New CloneOptions("@owner", temp.Value, TimeSpan.FromSeconds(seconds))
    End Function

    Private Shared Function MakeBatch(git As FakeGit, waits As List(Of TimeSpan), names As String(), Optional includeFork As Boolean = False) As CloneBatch
        Dim repos As List(Of Repository) = names.Select(Function(name As String) New Repository(name, False)).ToList()
        If includeFork Then repos.Add(New Repository("Fork", True))
        Return New CloneBatch(New FixedCatalog(repos), git,
            Function(wait As TimeSpan, cancellation As CancellationToken) As Task
                cancellation.ThrowIfCancellationRequested()
                waits.Add(wait)
                Return Task.CompletedTask
            End Function)
    End Function

    Private NotInheritable Class Recorder
        Implements IProgress(Of BatchUpdate)
        Public Sub Report(value As BatchUpdate) Implements IProgress(Of BatchUpdate).Report
            Assert.InRange(value.Processed, 0, value.Total)
        End Sub
    End Class

    Private NotInheritable Class FixedCatalog
        Implements IRepositoryCatalog
        Private ReadOnly repos As IReadOnlyList(Of Repository)
        Public Sub New(repos As IReadOnlyList(Of Repository))
            Me.repos = repos
        End Sub
        Public Function GetRepositoriesAsync(owner As String, report As Action(Of String), cancellation As CancellationToken) As Task(Of IReadOnlyList(Of Repository)) Implements IRepositoryCatalog.GetRepositoriesAsync
            Return Task.FromResult(repos)
        End Function
    End Class

    Private NotInheritable Class FakeGit
        Implements IGitClient
        Private ReadOnly clone As Func(Of Uri, String, CancellationToken, Task)
        Public Sub New(clone As Func(Of Uri, String, CancellationToken, Task))
            Me.clone = clone
        End Sub
        Public Function CheckAsync(cancellation As CancellationToken) As Task Implements IGitClient.CheckAsync
            Return Task.CompletedTask
        End Function
        Public Function CloneAsync(repositoryUri As Uri, target As String, report As Action(Of GitTransfer), cancellation As CancellationToken) As Task Implements IGitClient.CloneAsync
            report(New GitTransfer("Receiving objects", 60, "7.20 MiB", "1.00 MiB/s"))
            Return clone(repositoryUri, target, cancellation)
        End Function
    End Class

    Private NotInheritable Class TemporaryFolder
        Implements IDisposable
        Public ReadOnly Value As String = Path.Combine(Path.GetTempPath(), $"git-cloner-tests-{Guid.NewGuid():N}")
        Public Sub New()
            Directory.CreateDirectory(Value)
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            Directory.Delete(Value, recursive:=True)
        End Sub
    End Class
End Class
