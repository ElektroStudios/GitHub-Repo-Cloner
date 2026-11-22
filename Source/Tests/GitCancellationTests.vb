Option Strict On
Option Explicit On
Option Infer Off

Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Threading.Tasks
Imports GitHub_Repo_Cloner.Core

Public NotInheritable Class GitCancellationTests
    <Fact>
    Public Async Function CancelInterruptsGitWhileServerWithholdsItsResponse() As Task
        Dim server As New TcpListener(IPAddress.Loopback, 0)
        server.Start()
        Dim target As String = Path.Combine(Path.GetTempPath(), $"git-abort-{Guid.NewGuid():N}")
        Try
            Using cancellation As New CancellationTokenSource()
                Dim client As New GitClient()
                Await client.CheckAsync(cancellation.Token).ConfigureAwait(False)
                Dim port As Integer = DirectCast(server.LocalEndpoint, IPEndPoint).Port
                Dim clone As Task = client.CloneAsync(New Uri($"git://127.0.0.1:{port}/test.git"), target, Sub(update As GitTransfer) Return, cancellation.Token)
                Try
                    Dim connection As Task(Of TcpClient) = server.AcceptTcpClientAsync()
                    Assert.Same(connection, Await Task.WhenAny(connection, clone, Task.Delay(10000)).ConfigureAwait(False))
                    Using peer As TcpClient = Await connection.ConfigureAwait(False)
                        cancellation.Cancel()
                        Assert.Same(clone, Await Task.WhenAny(clone, Task.Delay(10000)).ConfigureAwait(False))
                        Await Assert.ThrowsAnyAsync(Of OperationCanceledException)(Function() clone).ConfigureAwait(False)
                    End Using
                Finally
                    cancellation.Cancel()
                End Try
            End Using
        Finally
            server.Stop()
            If Directory.Exists(target) Then Directory.Delete(target, recursive:=True)
        End Try
    End Function
End Class
