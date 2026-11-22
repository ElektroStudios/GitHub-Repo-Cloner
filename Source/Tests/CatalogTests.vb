Option Strict On
Option Explicit On
Option Infer Off

Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports GitHub_Repo_Cloner.Core

Public NotInheritable Class CatalogTests

    Private Shared ReadOnly Now As New DateTimeOffset(2026, 11, 22, 12, 0, 0, TimeSpan.Zero)

    <Fact>
    Public Sub InvalidTokenNeverAppearsInTheDiagnostic()
        Using client As New HttpClient()
            Dim token As String = $"secret{ChrW(10)}invalid"
            Dim failure As ArgumentException = Assert.Throws(Of ArgumentException)(Function() New GitHubCatalog(client, token))
            Assert.DoesNotContain("secret", failure.Message)
            Assert.Contains("token", failure.Message)
        End Using
    End Sub

    <Fact>
    Public Async Function FollowsPaginationAndKeepsForkFlagsAndDeduplicatesNames() As Task
        Dim requests As New List(Of Uri)()
        Dim waits As New List(Of TimeSpan)()
        Using client As New HttpClient(New FakeHttp(
            Function(request As HttpRequestMessage) As HttpResponseMessage
                Assert.Equal("api.github.com", request.RequestUri.Host)
                Assert.Equal("Bearer", request.Headers.Authorization.Scheme)
                Assert.Equal("test-secret", request.Headers.Authorization.Parameter)
                Assert.Equal("2022-11-28", request.Headers.GetValues("X-GitHub-Api-Version").Single())
                requests.Add(request.RequestUri)
                If requests.Count = 1 Then
                    Dim page As HttpResponseMessage = CreateResponse(200, "[{""name"":""First"",""fork"":false},{""name"":""Fork"",""fork"":true}]")
                    page.Headers.TryAddWithoutValidation("Link", "<https://different.invalid/>; rel=""next"", <https://api.github.com/>; rel=""last""")
                    Return page
                End If
                Return CreateResponse(200, "[{""name"":""first"",""fork"":false},{""name"":""Second"",""fork"":false}]")
            End Function))
            Dim catalog As GitHubCatalog = ClientFor(client, waits, "test-secret")
            Dim repositories As IReadOnlyList(Of Repository) = Await catalog.GetRepositoriesAsync("@Momo", Sub(message As String) Return, CancellationToken.None).ConfigureAwait(False)
            Assert.Collection(repositories, Sub(repo As Repository) Assert.Equal("First", repo.Name), Sub(repo As Repository) Assert.True(repo.IsFork AndAlso repo.Name = "Fork"), Sub(repo As Repository) Assert.Equal("Second", repo.Name))
            Assert.Equal(2, requests.Count)
            Assert.Contains("/users/Momo/repos", requests(0).AbsolutePath)
            Assert.EndsWith("page=2", requests(1).Query)
            Assert.Equal(TimeSpan.FromMilliseconds(500), Assert.Single(waits))
        End Using
    End Function

    <Fact>
    Public Async Function RetriesTransientStatusesAndUsesServerDelay() As Task
        Dim calls As Integer = 0
        Dim waits As New List(Of TimeSpan)()
        Using client As New HttpClient(New FakeHttp(
            Function(request As HttpRequestMessage) As HttpResponseMessage
                MatchNoAuth(request)
                calls += 1
                Select Case calls
                    Case 1
                        Dim response As HttpResponseMessage = CreateResponse(429, "{}")
                        response.Headers.RetryAfter = New RetryConditionHeaderValue(TimeSpan.FromSeconds(7))
                        Return response
                    Case 2
                        Return CreateResponse(503, "{}")
                    Case Else
                        Return CreateResponse(200, "[]")
                End Select
            End Function))
            Assert.Empty(Await ClientFor(client, waits).GetRepositoriesAsync("momo", Sub(message As String) Return, CancellationToken.None).ConfigureAwait(False))
            Assert.Collection(waits, Sub(wait As TimeSpan) Assert.Equal(TimeSpan.FromSeconds(7), wait),
                                      Sub(wait As TimeSpan) Assert.Equal(TimeSpan.FromSeconds(4), wait))
        End Using
    End Function

    <Fact>
    Public Async Function HonorsPrimaryRateLimitReset() As Task
        Dim waits As New List(Of TimeSpan)()
        Using client As New HttpClient(New FakeHttp(
            Function(request As HttpRequestMessage) As HttpResponseMessage
                If waits.Count > 0 Then Return CreateResponse(200, "[]")
                Dim response As HttpResponseMessage = CreateResponse(403, "{}")
                response.Headers.TryAddWithoutValidation("X-RateLimit-Remaining", "0")
                response.Headers.TryAddWithoutValidation("X-RateLimit-Reset", Now.AddSeconds(20).ToUnixTimeSeconds().ToString(Globalization.CultureInfo.InvariantCulture))
                Return response
            End Function))
            Assert.Empty(Await ClientFor(client, waits).GetRepositoriesAsync("momo", Sub(message As String) Return, CancellationToken.None).ConfigureAwait(False))
            Assert.Equal(TimeSpan.FromSeconds(21), Assert.Single(waits))
        End Using
    End Function

    <Fact>
    Public Async Function LongRateLimitFailsWithoutRetryingBeforeTheReset() As Task
        Dim waits As New List(Of TimeSpan)()
        Dim calls As Integer = 0
        Using client As New HttpClient(New FakeHttp(
            Function(request As HttpRequestMessage) As HttpResponseMessage
                calls += 1
                Dim response As HttpResponseMessage = CreateResponse(429, "{}")
                response.Headers.RetryAfter = New RetryConditionHeaderValue(Now.AddMinutes(30))
                Return response
            End Function))
            Dim failure As IOException = Await Assert.ThrowsAsync(Of IOException)(
                Function() ClientFor(client, waits).GetRepositoriesAsync("momo", Sub(message As String) Return, CancellationToken.None)).ConfigureAwait(False)
            Assert.Contains("rate limit", failure.Message)
            Assert.Equal(1, calls)
            Assert.Empty(waits)
        End Using
    End Function

    <Theory>
    <InlineData(401)>
    <InlineData(403)>
    <InlineData(404)>
    Public Async Function PermanentErrorsDoNotRetry(status As Integer) As Task
        Dim waits As New List(Of TimeSpan)()
        Dim calls As Integer = 0
        Using client As New HttpClient(New FakeHttp(
            Function(request As HttpRequestMessage) As HttpResponseMessage
                calls += 1
                Return CreateResponse(status, "{}")
            End Function))
            Await Assert.ThrowsAsync(Of IOException)(
                Function() ClientFor(client, waits).GetRepositoriesAsync("momo", Sub(message As String) Return, CancellationToken.None)).ConfigureAwait(False)
            Assert.Equal(1, calls)
            Assert.Empty(waits)
        End Using
    End Function

    <Theory>
    <InlineData("null")>
    <InlineData("[{""name"":""Repo""}]")>
    <InlineData("[{""name"":"""",""fork"":false}]")>
    <InlineData("[")>
    Public Async Function InvalidPayloadDoesNotBecomeAnEmptySuccessfulBatch(payload As String) As Task
        Dim waits As New List(Of TimeSpan)()
        Using client As New HttpClient(New FakeHttp(Function(request As HttpRequestMessage) CreateResponse(200, payload)))
            Await Assert.ThrowsAsync(Of InvalidDataException)(
                Function() ClientFor(client, waits).GetRepositoriesAsync("momo", Sub(message As String) Return, CancellationToken.None)).ConfigureAwait(False)
            Assert.Empty(waits)
        End Using
    End Function

    <Fact>
    Public Async Function RepeatedPaginationStops() As Task
        Dim waits As New List(Of TimeSpan)()
        Using client As New HttpClient(New FakeHttp(
            Function(request As HttpRequestMessage) As HttpResponseMessage
                Dim page As HttpResponseMessage = CreateResponse(200, "[{""name"":""Only"",""fork"":true}]")
                page.Headers.TryAddWithoutValidation("Link", "<https://api.github.com/>; rel=""next""")
                Return page
            End Function))
            Dim failure As InvalidDataException = Await Assert.ThrowsAsync(Of InvalidDataException)(
                Function() ClientFor(client, waits).GetRepositoriesAsync("momo", Sub(message As String) Return, CancellationToken.None)).ConfigureAwait(False)
            Assert.Contains("did not advance", failure.Message)
            Assert.Single(waits)
        End Using
    End Function

    <Fact>
    Public Async Function NetworkRetriesAreBounded() As Task
        Dim waits As New List(Of TimeSpan)()
        Dim calls As Integer = 0
        Using client As New HttpClient(New FakeHttp(
            Function(request As HttpRequestMessage) As HttpResponseMessage
                calls += 1
                Throw New HttpRequestException("offline")
            End Function))
            Await Assert.ThrowsAsync(Of IOException)(
                Function() ClientFor(client, waits).GetRepositoriesAsync("momo", Sub(message As String) Return, CancellationToken.None)).ConfigureAwait(False)
            Assert.Equal(4, calls)
            Assert.Equal(3, waits.Count)
        End Using
    End Function

    <Fact>
    Public Async Function CancelingRetryDelayStopsBeforeAnotherRequest() As Task
        Using cancellation As New CancellationTokenSource()
            Dim calls As Integer = 0
            Using client As New HttpClient(New FakeHttp(
                Function(request As HttpRequestMessage) As HttpResponseMessage
                    calls += 1
                    Return CreateResponse(503, "{}")
                End Function))
                Dim catalog As New GitHubCatalog(client, Nothing,
                    Function(wait As TimeSpan, token As CancellationToken) As Task
                        cancellation.Cancel()
                        Return Task.FromCanceled(token)
                    End Function, Function() Now)
                Await Assert.ThrowsAnyAsync(Of OperationCanceledException)(
                    Function() catalog.GetRepositoriesAsync("momo", Sub(message As String) Return, cancellation.Token)).ConfigureAwait(False)
                Assert.Equal(1, calls)
            End Using
        End Using
    End Function

    Private Shared Sub MatchNoAuth(request As HttpRequestMessage)
        Assert.Null(request.Headers.Authorization)
    End Sub

    Private Shared Function ClientFor(client As HttpClient, waits As List(Of TimeSpan), Optional token As String = Nothing) As GitHubCatalog
        Return New GitHubCatalog(client, token,
            Function(delay As TimeSpan, cancellation As CancellationToken) As Task
                cancellation.ThrowIfCancellationRequested()
                waits.Add(delay)
                Return Task.CompletedTask
            End Function, Function() Now)
    End Function

    Private Shared Function CreateResponse(status As Integer, json As String) As HttpResponseMessage
        Return New HttpResponseMessage(CType(status, HttpStatusCode)) With {.Content = New StringContent(json, Encoding.UTF8, "application/json")}
    End Function

    Private NotInheritable Class FakeHttp
        Inherits HttpMessageHandler
        Private ReadOnly respond As Func(Of HttpRequestMessage, HttpResponseMessage)
        Public Sub New(respond As Func(Of HttpRequestMessage, HttpResponseMessage))
            Me.respond = respond
        End Sub
        Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
            cancellationToken.ThrowIfCancellationRequested()
            Return Task.FromResult(respond(request))
        End Function
    End Class
End Class
