Option Strict On
Option Explicit On
Option Infer Off

Imports System.IO
Imports System.Net.Http
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports GitHub_Repo_Cloner.Core
Imports GitHub_Repo_Cloner.UI

''' <summary>The batch clone controls and the live activity dashboard.</summary>
Public Class Form1

    Private Shared ReadOnly DefaultStatus As Color = Color.FromArgb(139, 148, 158)
    Private Shared ReadOnly SuccessStatus As Color = Color.FromArgb(63, 185, 80)
    Private Shared ReadOnly WarningStatus As Color = Color.FromArgb(210, 153, 34)
    Private Shared ReadOnly FailureStatus As Color = Color.FromArgb(248, 81, 73)
    Private Const RepositoryUrl As String = "https://github.com/ElektroStudios/GitHub-Repo-Cloner"
    Private activeCancellation As CancellationTokenSource
    Private progress As ProgressBuffer
    Private activeRun As Task
    Private stopPromptOpen As Boolean
    Private closeRequested As Boolean
    Private repository As String
    Private phase As String
    Private percent As Integer
    Private receivedSize As String
    Private rate As String

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = $"GitHub Repo Cloner {My.Application.Info.Version.ToString(3)}"
        Using source As Stream = GetType(Form1).Assembly.GetManifestResourceStream("GitHub_Repo_Cloner.GitHub_Invertocat_White.png")
            If source Is Nothing Then Throw New InvalidOperationException("The GitHub project icon is missing.")
            Using original As Image = Image.FromStream(source)
                RepositoryButton.SetIcon(original)
            End Using
        End Using

        Dim documents As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify)
        If documents.Length = 0 Then
            Dim profile As String = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify)
            If profile.Length > 0 Then documents = Path.Combine(profile, "Documents")
        End If
        If documents.Length > 0 Then DestinationInput.Text = Path.Combine(documents, "GitHub")

        If Not SystemInformation.HighContrast Then
            Dim enabled As Integer = 1
            If DwmSetWindowAttribute(Me.Handle, 20, enabled, 4) <> 0 Then DwmSetWindowAttribute(Me.Handle, 19, enabled, 4)
        End If
    End Sub

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(window As IntPtr, attribute As Integer, ByRef value As Integer, length As Integer) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function SendMessage(window As IntPtr, message As Integer, wParam As IntPtr, text As String) As IntPtr
    End Function

    Private Sub OwnerInput_HandleCreated(sender As Object, e As EventArgs) Handles OwnerInput.HandleCreated
        Const SetCueBanner As Integer = &H1501
        SendMessage(OwnerInput.Handle, SetCueBanner, New IntPtr(1), "@Username")
    End Sub

    Private Sub RepositoryButton_Click(sender As Object, e As EventArgs) Handles RepositoryButton.Click
        Try
            Using browser As Process = Process.Start(New ProcessStartInfo(RepositoryUrl) With {.UseShellExecute = True})
            End Using
        Catch ex As Exception When TypeOf ex Is System.ComponentModel.Win32Exception OrElse TypeOf ex Is InvalidOperationException
            MessageBox.Show(Me, $"Could not open the GitHub repository: {ex.Message}", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub BrowseButton_Click(sender As Object, e As EventArgs) Handles BrowseButton.Click
        Using picker As New Ookii.Dialogs.WinForms.VistaFolderBrowserDialog With {.UseDescriptionForTitle = True, .Description = "Choose the root folder for cloned repositories.", .SelectedPath = If(Directory.Exists(DestinationInput.Text), DestinationInput.Text, String.Empty)}
            If picker.ShowDialog(Me) = DialogResult.OK Then DestinationInput.Text = picker.SelectedPath
        End Using
    End Sub

    Private Sub TokenButton_Click(sender As Object, e As EventArgs) Handles TokenButton.Click
        Using editor As New TokenDialog()
            editor.ShowDialog(Me)
        End Using
    End Sub

    Private Async Sub StartButton_Click(sender As Object, e As EventArgs) Handles StartButton.Click
        If activeRun IsNot Nothing OrElse closeRequested Then Return
        Try
            activeRun = RunBatchAsync()
            Await activeRun
        Finally
            activeRun = Nothing
        End Try
    End Sub

    Private Async Function RunBatchAsync() As Task
        Dim options As CloneOptions
        Try
            options = New CloneOptions(OwnerInput.Text, DestinationInput.Text, TimeSpan.FromSeconds(CDbl(DelayInput.Value)), ForksInput.Checked)
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is IOException OrElse TypeOf ex Is NotSupportedException
            SetStatus(ex.Message, WarningStatus)
            Return
        End Try

        DestinationInput.Text = options.DirectoryPath
        activeCancellation = New CancellationTokenSource()
        Dim cancellation As CancellationToken = activeCancellation.Token
        Dim updates As New ProgressBuffer()
        progress = updates
        SetRunning(True)
        ActivityLog.Clear()
        ClearProgress()
        SetStatus("Starting batch…", DefaultStatus)
        UpdateTimer.Start()

        Try
            Dim finalMessage As String
            Dim finalColor As Color
            Dim finalLog As String = Nothing
            Try
                Using handler As New HttpClientHandler With {.AllowAutoRedirect = False},
                      client As New HttpClient(handler, disposeHandler:=False) With {
                          .Timeout = TimeSpan.FromSeconds(30), .MaxResponseContentBufferSize = 8 * 1024 * 1024}
                    Dim batch As New CloneBatch(New GitHubCatalog(client, If(TokenStore.Current.ReadSaved(), Environment.GetEnvironmentVariable("GITHUB_TOKEN"))), New GitClient())
                    Dim result As BatchUpdate = Await Task.Run(Function() batch.RunAsync(options, updates, cancellation), cancellation)
                    finalMessage = If(result.Total = 0, "No repositories to clone.",
                        $"Finished  ·  {result.Cloned} cloned  ·  {result.Skipped} skipped  ·  {result.Failed} failed")
                    finalColor = If(result.Failed > 0, WarningStatus, SuccessStatus)
                End Using
            Catch ex As OperationCanceledException When cancellation.IsCancellationRequested
                finalMessage = ex.Message
                finalLog = finalMessage
                finalColor = WarningStatus
            Catch ex As Exception
                finalMessage = ex.Message
                finalLog = $"Error: {finalMessage}"
                finalColor = FailureStatus
            End Try

            While FlushProgress()
                Await Task.Yield()
            End While
            ClearTransfer()
            If finalLog IsNot Nothing Then AppendActivity(finalLog)
            SetStatus(finalMessage, finalColor)
        Finally
            UpdateTimer.Stop()
            activeCancellation.Dispose()
            activeCancellation = Nothing
            progress = Nothing
            If Not closeRequested Then SetRunning(False)
        End Try
    End Function

    Private Sub CancelRunButton_Click(sender As Object, e As EventArgs) Handles CancelRunButton.Click
        If activeCancellation Is Nothing OrElse activeCancellation.IsCancellationRequested OrElse stopPromptOpen Then Return
        If ConfirmStop(False) Then RequestCancellation()
    End Sub

    Private Async Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Dim pending As Task = activeRun
        If stopPromptOpen Then
            e.Cancel = True
            Return
        End If
        If pending Is Nothing OrElse pending.IsCompleted Then Return
        e.Cancel = True
        If closeRequested Then Return
        If e.CloseReason = CloseReason.UserClosing AndAlso Not ConfirmStop(True) Then Return
        closeRequested = True
        RequestCancellation()
        Await pending
        Me.BeginInvoke(New Action(AddressOf Me.Close))
    End Sub

    Private Function ConfirmStop(exitAfterStop As Boolean) As Boolean
        Dim question As String = "Stop the current batch now?"
        If exitAfterStop Then
            question = If(activeCancellation IsNot Nothing AndAlso activeCancellation.IsCancellationRequested,
                          "Cancellation is already in progress. Exit after cleanup?",
                          "Stop the current batch now and exit?")
        End If
        stopPromptOpen = True
        Try
            Return MessageBox.Show(Me, question, Me.Text, MessageBoxButtons.YesNo,
                                   MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) = DialogResult.Yes
        Finally
            stopPromptOpen = False
        End Try
    End Function

    Private Sub RequestCancellation()
        If activeCancellation Is Nothing Then Return
        CancelRunButton.Enabled = False
        SetStatus(If(closeRequested, "Stopping and cleaning up before closing…", "Stopping and cleaning up…"), WarningStatus)
        If Not activeCancellation.IsCancellationRequested Then activeCancellation.Cancel()
    End Sub

    Private Sub SetRunning(running As Boolean)
        ForksInput.Enabled = Not running
        TokenButton.Enabled = Not running
        OwnerInput.Enabled = Not running
        DestinationInput.Enabled = Not running
        DelayInput.Enabled = Not running
        BrowseButton.Enabled = Not running
        StartButton.Enabled = Not running
        CancelRunButton.Enabled = running
    End Sub

    Private Sub ActionButton_EnabledChanged(sender As Object, e As EventArgs) Handles StartButton.EnabledChanged, CancelRunButton.EnabledChanged
        Dim button As Button = DirectCast(sender, Button)
        If Not button.Enabled Then
            button.BackColor = Color.FromArgb(33, 38, 45)
            button.FlatAppearance.BorderColor = Color.FromArgb(48, 54, 61)
        ElseIf button Is Me.StartButton Then
            button.BackColor = Color.FromArgb(38, 69, 102)
            button.FlatAppearance.BorderColor = Color.FromArgb(65, 100, 136)
        Else
            button.BackColor = Color.FromArgb(119, 53, 57)
            button.FlatAppearance.BorderColor = Color.IndianRed
        End If
    End Sub

    Private Sub UpdateTimer_Tick(sender As Object, e As EventArgs) Handles UpdateTimer.Tick
        FlushProgress()
    End Sub

    Private Function FlushProgress() As Boolean
        Dim update As BatchUpdate = Nothing
        Dim latest As BatchUpdate = Nothing
        Dim activity As New StringBuilder()
        Dim latestMessage As String = Nothing
        For index As Integer = 1 To 300
            If Not progress.TryDequeue(update) Then Exit For
            latest = update
            If repository <> update.RepositoryName OrElse
               (update.Message IsNot Nothing AndAlso update.Message.StartsWith("Cloning ", StringComparison.Ordinal)) Then
                repository = update.RepositoryName
                phase = Nothing
                percent = 0
                receivedSize = Nothing
                rate = Nothing
            End If
            If update.Transfer IsNot Nothing Then
                phase = update.Transfer.Phase
                percent = update.Transfer.Percent
                If update.Transfer.Size.Length > 0 Then receivedSize = update.Transfer.Size
                If update.Transfer.Rate.Length > 0 Then rate = update.Transfer.Rate
            End If
            If update.Message IsNot Nothing Then
                Dim tag As String
                Select Case update.Kind
                    Case BatchMessageKind.Success : tag = " OK "
                    Case BatchMessageKind.Warning : tag = "WARN"
                    Case BatchMessageKind.Failure : tag = "FAIL"
                    Case Else : tag = "INFO"
                End Select
                activity.AppendLine($"{DateTime.Now:HH:mm:ss}  {tag}  {update.Message}")
                latestMessage = update.Message
            End If
        Next

        If latest Is Nothing Then Return False
        If latestMessage IsNot Nothing AndAlso Not activeCancellation.IsCancellationRequested Then
            SetStatus(latestMessage, DefaultStatus)
        End If
        If activity.Length > 0 Then
            ActivityLog.AppendText(activity.ToString())
            ActivityLog.ScrollToCaret()
        End If
        TotalCount.Text = $"Total  {latest.Total:N0}"
        ClonedCount.Text = $"Cloned  {latest.Cloned:N0}"
        SkippedCount.Text = $"Skipped  {latest.Skipped:N0}"
        FailedCount.Text = $"Failed  {latest.Failed:N0}"
        BatchCounter.Text = $"{latest.Processed:N0} / {latest.Total:N0} processed"
        BatchBar.UpdateProgress(latest.Processed, latest.Total)

        If repository Is Nothing OrElse latest.Processed = latest.Total Then
            ClearTransfer()
        Else
            CurrentRepo.Text = If(phase Is Nothing, repository, $"{repository}  ·  {phase} {percent}%")
            TransferStats.Text = $"Received {If(receivedSize, "—")}  ·  Rate {If(rate, "—")}"
            TransferBar.UpdateProgress(percent, 100)
        End If
        Return progress.HasPending
    End Function

    Private Sub ClearProgress()
        TotalCount.Text = "Total  —"
        ClonedCount.Text = "Cloned  0"
        SkippedCount.Text = "Skipped  0"
        FailedCount.Text = "Failed  0"
        BatchCounter.Text = "0 processed"
        BatchBar.UpdateProgress(0, 1)
        ClearTransfer()
    End Sub

    Private Sub ClearTransfer()
        repository = Nothing
        phase = Nothing
        percent = 0
        receivedSize = Nothing
        rate = Nothing
        CurrentRepo.Text = "No active repository"
        TransferStats.Text = "Received —  ·  Rate —"
        TransferBar.UpdateProgress(0, 100)
    End Sub

    Private Sub AppendActivity(message As String)
        ActivityLog.AppendText($"{DateTime.Now:HH:mm:ss}  {message}{Environment.NewLine}")
        ActivityLog.ScrollToCaret()
    End Sub

    Private Sub SetStatus(message As String, color As Color)
        StatusLabel.Text = message
        StatusLabel.ForeColor = color
    End Sub

End Class
