Option Strict On
Option Explicit On
Option Infer Off

Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>Lists public repositories through GitHub's paginated REST endpoint.</summary>
Public NotInheritable Class GitHubCatalog
    Implements IRepositoryCatalog

    Private Shared ReadOnly NextLink As New Regex(";\s*rel\s*=\s*""?next""?\s*(?:;|,|$)", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private Shared ReadOnly TokenPattern As New Regex("\A[A-Za-z0-9._~+/=-]+\z", RegexOptions.CultureInvariant)
    Private Shared ReadOnly MaximumWait As TimeSpan = TimeSpan.FromMinutes(2)
    Private ReadOnly client As HttpClient
    Private ReadOnly token As String
    Private ReadOnly pause As Func(Of TimeSpan, CancellationToken, Task)
    Private ReadOnly clock As Func(Of DateTimeOffset)

    Public Sub New(client As HttpClient, token As String)
        Me.New(client, token, AddressOf Task.Delay, Function() DateTimeOffset.UtcNow)
    End Sub

    Friend Sub New(client As HttpClient, token As String, pause As Func(Of TimeSpan, CancellationToken, Task), clock As Func(Of DateTimeOffset))
        If client Is Nothing Then Throw New ArgumentNullException(NameOf(client))
        If pause Is Nothing Then Throw New ArgumentNullException(NameOf(pause))
        If clock Is Nothing Then Throw New ArgumentNullException(NameOf(clock))
        Me.client = client
        Me.token = If(token, String.Empty)
        If Me.token.Length > 0 Then ValidateToken(Me.token)
        Me.pause = pause
        Me.clock = clock
    End Sub

    Public Shared Sub ValidateToken(token As String)
        If String.IsNullOrEmpty(token) OrElse Not TokenPattern.IsMatch(token) Then
            Throw New ArgumentException("Enter a token without spaces or control characters.", NameOf(token))
        End If
    End Sub

    Public Async Function GetRepositoriesAsync(owner As String, report As Action(Of String), cancellation As CancellationToken) As Task(Of IReadOnlyList(Of Repository)) Implements IRepositoryCatalog.GetRepositoriesAsync
        owner = CloneOptions.NormalizeOwner(owner)
        If report Is Nothing Then Throw New ArgumentNullException(NameOf(report))

        Dim repositories As New List(Of Repository)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For page As Integer = 1 To 1000
            cancellation.ThrowIfCancellationRequested()
            Dim uri As New Uri(FormattableString.Invariant($"https://api.github.com/users/{owner}/repos?type=owner&per_page=100&sort=full_name&page={page}"))
            report($"Reading GitHub page {page}…")
            Using response As HttpResponseMessage = Await FetchAsync(uri, report, cancellation).ConfigureAwait(False)
                Dim records As RepositoryRecord()
                Using payload As Stream = Await response.Content.ReadAsStreamAsync().ConfigureAwait(False)
                    cancellation.ThrowIfCancellationRequested()
                    Dim serializer As New DataContractJsonSerializer(GetType(RepositoryRecord()))
                    Try
                        records = DirectCast(serializer.ReadObject(payload), RepositoryRecord())
                    Catch ex As SerializationException
                        Throw New InvalidDataException("GitHub returned an unreadable repository list.", ex)
                    End Try
                End Using
                If records Is Nothing Then Throw New InvalidDataException("GitHub returned an empty JSON value for its repository list.")
                Dim newRecords As Integer = 0
                For Each item As RepositoryRecord In records
                    If item Is Nothing OrElse String.IsNullOrWhiteSpace(item.Name) Then
                        Throw New InvalidDataException("GitHub returned a repository without a name.")
                    End If
                    If seen.Add(item.Name) Then
                        newRecords += 1
                        repositories.Add(New Repository(item.Name, item.Fork))
                    End If
                Next
                Dim links As IEnumerable(Of String) = Nothing
                If Not response.Headers.TryGetValues("Link", links) OrElse Not links.Any(Function(link As String) NextLink.IsMatch(link)) Then
                    Return repositories.AsReadOnly()
                End If
                If newRecords = 0 Then Throw New InvalidDataException("GitHub pagination did not advance.")
            End Using
            Await pause(TimeSpan.FromMilliseconds(500), cancellation).ConfigureAwait(False)
        Next

        Throw New InvalidDataException("GitHub pagination exceeded the page limit.")
    End Function

    Private Async Function FetchAsync(uri As Uri, report As Action(Of String), cancellation As CancellationToken) As Task(Of HttpResponseMessage)
        For attempt As Integer = 1 To 4
            cancellation.ThrowIfCancellationRequested()
            Dim wait As TimeSpan = TimeSpan.FromSeconds(Math.Pow(2, attempt))
            Dim problem As Exception = Nothing
            Try
                Using request As New HttpRequestMessage(HttpMethod.Get, uri)
                    request.Headers.UserAgent.ParseAdd("GitHubRepoCloner/1.0")
                    request.Headers.Accept.ParseAdd("application/vnd.github+json")
                    request.Headers.Add("X-GitHub-Api-Version", "2022-11-28")
                    If token.Length > 0 Then request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", token)
                    Dim response As HttpResponseMessage = Await client.SendAsync(request, cancellation).ConfigureAwait(False)
                    If response.IsSuccessStatusCode Then Return response
                    Using response
                        Dim status As Integer = CInt(response.StatusCode)
                        Dim remaining As String = Header(response, "X-RateLimit-Remaining")
                        Dim throttled As Boolean = status = 429 OrElse
                            (status = 403 AndAlso (remaining = "0" OrElse response.Headers.RetryAfter IsNot Nothing OrElse
                            (response.Content IsNot Nothing AndAlso
                            (Await response.Content.ReadAsStringAsync().ConfigureAwait(False)).IndexOf("rate limit", StringComparison.OrdinalIgnoreCase) >= 0)))

                        If throttled Then
                            wait = RateLimitWait(response, remaining)
                            problem = New IOException($"GitHub rate limit reached. Retry after {clock().Add(wait).ToLocalTime():g}.")
                        ElseIf status = 408 OrElse status = 500 OrElse status = 502 OrElse status = 503 OrElse status = 504 Then
                            wait = If(response.Headers.RetryAfter Is Nothing, wait, RetryAfter(response.Headers.RetryAfter))
                            problem = New IOException($"GitHub returned HTTP {status} ({response.ReasonPhrase}).")
                        Else
                            Throw New IOException(PermanentError(status, response.ReasonPhrase))
                        End If
                    End Using
                End Using
            Catch ex As HttpRequestException
                problem = New IOException("A connection to GitHub could not be completed.", ex)
            Catch ex As OperationCanceledException When Not cancellation.IsCancellationRequested
                problem = New IOException("GitHub did not respond before the request timed out.", ex)
            End Try

            If attempt = 4 OrElse wait > MaximumWait Then Throw problem
            report($"{problem.Message} Retrying in {Math.Ceiling(wait.TotalSeconds)} s…")
            Await pause(wait, cancellation).ConfigureAwait(False)
        Next
        Throw New InvalidOperationException("The API retry loop did not complete.")
    End Function

    Private Function RateLimitWait(response As HttpResponseMessage, remaining As String) As TimeSpan
        If response.Headers.RetryAfter IsNot Nothing Then Return RetryAfter(response.Headers.RetryAfter)
        Dim reset As Long
        If remaining = "0" AndAlso Long.TryParse(Header(response, "X-RateLimit-Reset"), NumberStyles.None, CultureInfo.InvariantCulture, reset) AndAlso
           reset <= 253402300798L Then
            Return AtLeastOneSecond(DateTimeOffset.FromUnixTimeSeconds(reset).AddSeconds(1) - clock())
        End If
        Return TimeSpan.FromMinutes(1)
    End Function

    Private Function RetryAfter(header As RetryConditionHeaderValue) As TimeSpan
        Return AtLeastOneSecond(If(header.Delta, header.Date.GetValueOrDefault() - clock()))
    End Function

    Private Shared Function AtLeastOneSecond(value As TimeSpan) As TimeSpan
        Return If(value < TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), value)
    End Function

    Private Shared Function Header(response As HttpResponseMessage, name As String) As String
        Dim values As IEnumerable(Of String) = Nothing
        Return If(response.Headers.TryGetValues(name, values), values.FirstOrDefault(), Nothing)
    End Function

    Private Shared Function PermanentError(status As Integer, reason As String) As String
        Select Case status
            Case 401 : Return "GitHub rejected the API token. Check your token in API token settings."
            Case 403 : Return "GitHub denied access to the repository list (HTTP 403)."
            Case 404 : Return "GitHub could not find this account or its public repository list (HTTP 404)."
            Case Else : Return $"GitHub returned HTTP {status} ({reason})."
        End Select
    End Function

    <DataContract>
    Private NotInheritable Class RepositoryRecord
        <DataMember(Name:="name", IsRequired:=True)>
        Public Property Name As String
        <DataMember(Name:="fork", IsRequired:=True)>
        Public Property Fork As Boolean
    End Class
End Class
