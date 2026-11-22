Option Strict On
Option Explicit On
Option Infer Off

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits Form

    <DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                If Me.components IsNot Nothing Then Me.components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.TokenButton = New System.Windows.Forms.Button()
        Me.ForksInput = New GitHub_Repo_Cloner.UI.ForkCheckBox()
        Me.Subtitle = New System.Windows.Forms.Label()
        Me.RepositoryTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.RepositoryButton = New GitHub_Repo_Cloner.UI.IconButton()
        Me.OptionsPanel = New System.Windows.Forms.Panel()
        Me.OwnerLabel = New System.Windows.Forms.Label()
        Me.OwnerInput = New System.Windows.Forms.TextBox()
        Me.DelayLabel = New System.Windows.Forms.Label()
        Me.DelayInput = New System.Windows.Forms.NumericUpDown()
        Me.StartButton = New System.Windows.Forms.Button()
        Me.CancelRunButton = New System.Windows.Forms.Button()
        Me.DestinationLabel = New System.Windows.Forms.Label()
        Me.DestinationInput = New System.Windows.Forms.TextBox()
        Me.BrowseButton = New System.Windows.Forms.Button()
        Me.ProgressPanel = New System.Windows.Forms.Panel()
        Me.BatchHeading = New System.Windows.Forms.Label()
        Me.BatchCounter = New System.Windows.Forms.Label()
        Me.TotalCount = New System.Windows.Forms.Label()
        Me.ClonedCount = New System.Windows.Forms.Label()
        Me.SkippedCount = New System.Windows.Forms.Label()
        Me.FailedCount = New System.Windows.Forms.Label()
        Me.BatchBar = New GitHub_Repo_Cloner.UI.ProgressTrack()
        Me.CurrentRepo = New System.Windows.Forms.Label()
        Me.TransferStats = New System.Windows.Forms.Label()
        Me.TransferBar = New GitHub_Repo_Cloner.UI.ProgressTrack()
        Me.ActivityPanel = New System.Windows.Forms.Panel()
        Me.ActivityLabel = New System.Windows.Forms.Label()
        Me.ActivityLog = New System.Windows.Forms.RichTextBox()
        Me.StatusLabel = New System.Windows.Forms.Label()
        Me.UpdateTimer = New System.Windows.Forms.Timer(Me.components)
        Me.OptionsPanel.SuspendLayout()
        CType(Me.DelayInput, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ProgressPanel.SuspendLayout()
        Me.ActivityPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'TokenButton
        '
        Me.TokenButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TokenButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(54, Byte), Integer), CType(CType(61, Byte), Integer))
        Me.TokenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.TokenButton.ForeColor = System.Drawing.Color.Gainsboro
        Me.TokenButton.Location = New System.Drawing.Point(640, 11)
        Me.TokenButton.Name = "TokenButton"
        Me.TokenButton.Size = New System.Drawing.Size(136, 34)
        Me.TokenButton.TabIndex = 6
        Me.TokenButton.Text = "API token…"
        '
        'ForksInput
        '
        Me.ForksInput.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ForksInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ForksInput.Location = New System.Drawing.Point(490, 11)
        Me.ForksInput.Name = "ForksInput"
        Me.ForksInput.Size = New System.Drawing.Size(140, 34)
        Me.ForksInput.TabIndex = 5
        Me.ForksInput.Text = "Include forks"
        '
        'Subtitle
        '
        Me.Subtitle.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Subtitle.AutoEllipsis = True
        Me.Subtitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.Subtitle.Location = New System.Drawing.Point(22, 12)
        Me.Subtitle.Name = "Subtitle"
        Me.Subtitle.Size = New System.Drawing.Size(430, 34)
        Me.Subtitle.TabIndex = 4
        Me.Subtitle.Text = "Clone an account’s public repositories."
        Me.Subtitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'RepositoryButton
        '
        Me.RepositoryButton.AccessibleName = "Open the project on GitHub"
        Me.RepositoryButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton
        Me.RepositoryButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.RepositoryButton.BackColor = System.Drawing.Color.Transparent
        Me.RepositoryButton.Cursor = System.Windows.Forms.Cursors.Hand
        Me.RepositoryButton.Location = New System.Drawing.Point(792, 10)
        Me.RepositoryButton.Name = "RepositoryButton"
        Me.RepositoryButton.Size = New System.Drawing.Size(36, 36)
        Me.RepositoryButton.TabIndex = 7
        Me.RepositoryTip.SetToolTip(Me.RepositoryButton, "Open the project on GitHub")
        '
        'OptionsPanel
        '
        Me.OptionsPanel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.OptionsPanel.BackColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(34, Byte), Integer))
        Me.OptionsPanel.Controls.Add(Me.OwnerLabel)
        Me.OptionsPanel.Controls.Add(Me.OwnerInput)
        Me.OptionsPanel.Controls.Add(Me.DelayLabel)
        Me.OptionsPanel.Controls.Add(Me.DelayInput)
        Me.OptionsPanel.Controls.Add(Me.StartButton)
        Me.OptionsPanel.Controls.Add(Me.CancelRunButton)
        Me.OptionsPanel.Controls.Add(Me.DestinationLabel)
        Me.OptionsPanel.Controls.Add(Me.DestinationInput)
        Me.OptionsPanel.Controls.Add(Me.BrowseButton)
        Me.OptionsPanel.Location = New System.Drawing.Point(22, 57)
        Me.OptionsPanel.Name = "OptionsPanel"
        Me.OptionsPanel.Size = New System.Drawing.Size(806, 165)
        Me.OptionsPanel.TabIndex = 0
        '
        'OwnerLabel
        '
        Me.OwnerLabel.AutoEllipsis = True
        Me.OwnerLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.OwnerLabel.Location = New System.Drawing.Point(17, 12)
        Me.OwnerLabel.Name = "OwnerLabel"
        Me.OwnerLabel.Size = New System.Drawing.Size(185, 24)
        Me.OwnerLabel.TabIndex = 0
        Me.OwnerLabel.Text = "GitHub username"
        Me.OwnerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.OwnerLabel.UseMnemonic = False
        '
        'OwnerInput
        '
        Me.OwnerInput.AccessibleName = "GitHub username"
        Me.OwnerInput.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(17, Byte), Integer), CType(CType(23, Byte), Integer))
        Me.OwnerInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.OwnerInput.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.OwnerInput.Location = New System.Drawing.Point(18, 41)
        Me.OwnerInput.Name = "OwnerInput"
        Me.OwnerInput.Size = New System.Drawing.Size(196, 26)
        Me.OwnerInput.TabIndex = 1
        '
        'DelayLabel
        '
        Me.DelayLabel.AutoEllipsis = True
        Me.DelayLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.DelayLabel.Location = New System.Drawing.Point(227, 12)
        Me.DelayLabel.Name = "DelayLabel"
        Me.DelayLabel.Size = New System.Drawing.Size(280, 24)
        Me.DelayLabel.TabIndex = 2
        Me.DelayLabel.Text = "Delay between downloads (in seconds)"
        Me.DelayLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.DelayLabel.UseMnemonic = False
        '
        'DelayInput
        '
        Me.DelayInput.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(17, Byte), Integer), CType(CType(23, Byte), Integer))
        Me.DelayInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.DelayInput.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.DelayInput.Location = New System.Drawing.Point(228, 41)
        Me.DelayInput.Maximum = New Decimal(New Integer() {3600, 0, 0, 0})
        Me.DelayInput.Name = "DelayInput"
        Me.DelayInput.Size = New System.Drawing.Size(112, 26)
        Me.DelayInput.TabIndex = 3
        Me.DelayInput.Value = New Decimal(New Integer() {2, 0, 0, 0})
        '
        'StartButton
        '
        Me.StartButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.StartButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(38, Byte), Integer), CType(CType(69, Byte), Integer), CType(CType(102, Byte), Integer))
        Me.StartButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(100, Byte), Integer), CType(CType(136, Byte), Integer))
        Me.StartButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(56, Byte), Integer), CType(CType(86, Byte), Integer))
        Me.StartButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(83, Byte), Integer), CType(CType(121, Byte), Integer))
        Me.StartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.StartButton.ForeColor = System.Drawing.Color.White
        Me.StartButton.Location = New System.Drawing.Point(508, 35)
        Me.StartButton.Name = "StartButton"
        Me.StartButton.Size = New System.Drawing.Size(152, 40)
        Me.StartButton.TabIndex = 4
        Me.StartButton.Text = "&Start cloning"
        Me.StartButton.UseVisualStyleBackColor = False
        '
        'CancelRunButton
        '
        Me.CancelRunButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CancelRunButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(38, Byte), Integer), CType(CType(45, Byte), Integer))
        Me.CancelRunButton.Enabled = False
        Me.CancelRunButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(54, Byte), Integer), CType(CType(61, Byte), Integer))
        Me.CancelRunButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(106, Byte), Integer), CType(CType(47, Byte), Integer), CType(CType(51, Byte), Integer))
        Me.CancelRunButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(149, Byte), Integer), CType(CType(69, Byte), Integer), CType(CType(74, Byte), Integer))
        Me.CancelRunButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.CancelRunButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.CancelRunButton.Location = New System.Drawing.Point(665, 35)
        Me.CancelRunButton.Name = "CancelRunButton"
        Me.CancelRunButton.Size = New System.Drawing.Size(123, 40)
        Me.CancelRunButton.TabIndex = 5
        Me.CancelRunButton.Text = "&Cancel"
        Me.CancelRunButton.UseVisualStyleBackColor = False
        '
        'DestinationLabel
        '
        Me.DestinationLabel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.DestinationLabel.AutoEllipsis = True
        Me.DestinationLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.DestinationLabel.Location = New System.Drawing.Point(17, 89)
        Me.DestinationLabel.Name = "DestinationLabel"
        Me.DestinationLabel.Size = New System.Drawing.Size(642, 24)
        Me.DestinationLabel.TabIndex = 6
        Me.DestinationLabel.Text = "Destination folder"
        Me.DestinationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.DestinationLabel.UseMnemonic = False
        '
        'DestinationInput
        '
        Me.DestinationInput.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.DestinationInput.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(17, Byte), Integer), CType(CType(23, Byte), Integer))
        Me.DestinationInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.DestinationInput.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.DestinationInput.Location = New System.Drawing.Point(18, 120)
        Me.DestinationInput.Name = "DestinationInput"
        Me.DestinationInput.Size = New System.Drawing.Size(641, 26)
        Me.DestinationInput.TabIndex = 7
        '
        'BrowseButton
        '
        Me.BrowseButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BrowseButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(38, Byte), Integer), CType(CType(45, Byte), Integer))
        Me.BrowseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(54, Byte), Integer), CType(CType(61, Byte), Integer))
        Me.BrowseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(40, Byte), Integer), CType(CType(46, Byte), Integer), CType(CType(51, Byte), Integer))
        Me.BrowseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(54, Byte), Integer), CType(CType(61, Byte), Integer))
        Me.BrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BrowseButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.BrowseButton.Location = New System.Drawing.Point(665, 114)
        Me.BrowseButton.Name = "BrowseButton"
        Me.BrowseButton.Size = New System.Drawing.Size(123, 36)
        Me.BrowseButton.TabIndex = 8
        Me.BrowseButton.Text = "&Browse…"
        Me.BrowseButton.UseVisualStyleBackColor = False
        '
        'ProgressPanel
        '
        Me.ProgressPanel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ProgressPanel.BackColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(34, Byte), Integer))
        Me.ProgressPanel.Controls.Add(Me.BatchHeading)
        Me.ProgressPanel.Controls.Add(Me.BatchCounter)
        Me.ProgressPanel.Controls.Add(Me.TotalCount)
        Me.ProgressPanel.Controls.Add(Me.ClonedCount)
        Me.ProgressPanel.Controls.Add(Me.SkippedCount)
        Me.ProgressPanel.Controls.Add(Me.FailedCount)
        Me.ProgressPanel.Controls.Add(Me.BatchBar)
        Me.ProgressPanel.Controls.Add(Me.CurrentRepo)
        Me.ProgressPanel.Controls.Add(Me.TransferStats)
        Me.ProgressPanel.Controls.Add(Me.TransferBar)
        Me.ProgressPanel.Location = New System.Drawing.Point(22, 231)
        Me.ProgressPanel.Name = "ProgressPanel"
        Me.ProgressPanel.Size = New System.Drawing.Size(806, 158)
        Me.ProgressPanel.TabIndex = 1
        '
        'BatchHeading
        '
        Me.BatchHeading.AutoEllipsis = True
        Me.BatchHeading.Font = New System.Drawing.Font("Segoe UI Semibold", 12.0!)
        Me.BatchHeading.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.BatchHeading.Location = New System.Drawing.Point(17, 10)
        Me.BatchHeading.Name = "BatchHeading"
        Me.BatchHeading.Size = New System.Drawing.Size(290, 27)
        Me.BatchHeading.TabIndex = 0
        Me.BatchHeading.Text = "Batch progress"
        Me.BatchHeading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'BatchCounter
        '
        Me.BatchCounter.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BatchCounter.AutoEllipsis = True
        Me.BatchCounter.ForeColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.BatchCounter.Location = New System.Drawing.Point(479, 10)
        Me.BatchCounter.Name = "BatchCounter"
        Me.BatchCounter.Size = New System.Drawing.Size(309, 27)
        Me.BatchCounter.TabIndex = 1
        Me.BatchCounter.Text = "0 processed"
        Me.BatchCounter.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'TotalCount
        '
        Me.TotalCount.AutoEllipsis = True
        Me.TotalCount.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.TotalCount.Location = New System.Drawing.Point(18, 40)
        Me.TotalCount.Name = "TotalCount"
        Me.TotalCount.Size = New System.Drawing.Size(123, 23)
        Me.TotalCount.TabIndex = 2
        Me.TotalCount.Text = "Total  —"
        Me.TotalCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ClonedCount
        '
        Me.ClonedCount.AutoEllipsis = True
        Me.ClonedCount.ForeColor = System.Drawing.Color.FromArgb(CType(CType(63, Byte), Integer), CType(CType(185, Byte), Integer), CType(CType(80, Byte), Integer))
        Me.ClonedCount.Location = New System.Drawing.Point(153, 40)
        Me.ClonedCount.Name = "ClonedCount"
        Me.ClonedCount.Size = New System.Drawing.Size(134, 23)
        Me.ClonedCount.TabIndex = 3
        Me.ClonedCount.Text = "Cloned  0"
        Me.ClonedCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'SkippedCount
        '
        Me.SkippedCount.AutoEllipsis = True
        Me.SkippedCount.ForeColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.SkippedCount.Location = New System.Drawing.Point(299, 40)
        Me.SkippedCount.Name = "SkippedCount"
        Me.SkippedCount.Size = New System.Drawing.Size(142, 23)
        Me.SkippedCount.TabIndex = 4
        Me.SkippedCount.Text = "Skipped  0"
        Me.SkippedCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'FailedCount
        '
        Me.FailedCount.AutoEllipsis = True
        Me.FailedCount.ForeColor = System.Drawing.Color.FromArgb(CType(CType(248, Byte), Integer), CType(CType(81, Byte), Integer), CType(CType(73, Byte), Integer))
        Me.FailedCount.Location = New System.Drawing.Point(453, 40)
        Me.FailedCount.Name = "FailedCount"
        Me.FailedCount.Size = New System.Drawing.Size(130, 23)
        Me.FailedCount.TabIndex = 5
        Me.FailedCount.Text = "Failed  0"
        Me.FailedCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'BatchBar
        '
        Me.BatchBar.AccessibleName = "Batch completion"
        Me.BatchBar.AccessibleRole = System.Windows.Forms.AccessibleRole.ProgressBar
        Me.BatchBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BatchBar.BackColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(54, Byte), Integer), CType(CType(61, Byte), Integer))
        Me.BatchBar.ForeColor = System.Drawing.Color.FromArgb(CType(CType(63, Byte), Integer), CType(CType(185, Byte), Integer), CType(CType(80, Byte), Integer))
        Me.BatchBar.Location = New System.Drawing.Point(18, 68)
        Me.BatchBar.Name = "BatchBar"
        Me.BatchBar.Size = New System.Drawing.Size(770, 10)
        Me.BatchBar.TabIndex = 7
        Me.BatchBar.TabStop = False
        '
        'CurrentRepo
        '
        Me.CurrentRepo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CurrentRepo.AutoEllipsis = True
        Me.CurrentRepo.Font = New System.Drawing.Font("Segoe UI Semibold", 12.0!)
        Me.CurrentRepo.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.CurrentRepo.Location = New System.Drawing.Point(18, 81)
        Me.CurrentRepo.Name = "CurrentRepo"
        Me.CurrentRepo.Size = New System.Drawing.Size(770, 31)
        Me.CurrentRepo.TabIndex = 6
        Me.CurrentRepo.Text = "No active repository"
        Me.CurrentRepo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TransferStats
        '
        Me.TransferStats.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TransferStats.AutoEllipsis = True
        Me.TransferStats.ForeColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.TransferStats.Location = New System.Drawing.Point(18, 111)
        Me.TransferStats.Name = "TransferStats"
        Me.TransferStats.Size = New System.Drawing.Size(770, 25)
        Me.TransferStats.TabIndex = 8
        Me.TransferStats.Text = "Received —  ·  Rate —"
        Me.TransferStats.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TransferBar
        '
        Me.TransferBar.AccessibleName = "Git phase progress"
        Me.TransferBar.AccessibleRole = System.Windows.Forms.AccessibleRole.ProgressBar
        Me.TransferBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TransferBar.BackColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(54, Byte), Integer), CType(CType(61, Byte), Integer))
        Me.TransferBar.ForeColor = System.Drawing.Color.FromArgb(CType(CType(88, Byte), Integer), CType(CType(166, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.TransferBar.Location = New System.Drawing.Point(18, 143)
        Me.TransferBar.Name = "TransferBar"
        Me.TransferBar.Size = New System.Drawing.Size(770, 5)
        Me.TransferBar.TabIndex = 9
        Me.TransferBar.TabStop = False
        '
        'ActivityPanel
        '
        Me.ActivityPanel.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ActivityPanel.BackColor = System.Drawing.Color.FromArgb(CType(CType(22, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(34, Byte), Integer))
        Me.ActivityPanel.Controls.Add(Me.ActivityLabel)
        Me.ActivityPanel.Controls.Add(Me.ActivityLog)
        Me.ActivityPanel.Location = New System.Drawing.Point(22, 398)
        Me.ActivityPanel.Name = "ActivityPanel"
        Me.ActivityPanel.Size = New System.Drawing.Size(806, 168)
        Me.ActivityPanel.TabIndex = 2
        '
        'ActivityLabel
        '
        Me.ActivityLabel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ActivityLabel.AutoEllipsis = True
        Me.ActivityLabel.Font = New System.Drawing.Font("Segoe UI Semibold", 12.0!)
        Me.ActivityLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.ActivityLabel.Location = New System.Drawing.Point(17, 8)
        Me.ActivityLabel.Name = "ActivityLabel"
        Me.ActivityLabel.Size = New System.Drawing.Size(773, 28)
        Me.ActivityLabel.TabIndex = 0
        Me.ActivityLabel.Text = "Activity"
        Me.ActivityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ActivityLog
        '
        Me.ActivityLog.AccessibleName = "Activity log"
        Me.ActivityLog.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ActivityLog.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(17, Byte), Integer), CType(CType(23, Byte), Integer))
        Me.ActivityLog.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.ActivityLog.DetectUrls = False
        Me.ActivityLog.Font = New System.Drawing.Font("Consolas", 10.0!)
        Me.ActivityLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.ActivityLog.Location = New System.Drawing.Point(18, 43)
        Me.ActivityLog.MaxLength = 0
        Me.ActivityLog.Name = "ActivityLog"
        Me.ActivityLog.ReadOnly = True
        Me.ActivityLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
        Me.ActivityLog.Size = New System.Drawing.Size(770, 110)
        Me.ActivityLog.TabIndex = 1
        Me.ActivityLog.Text = ""
        '
        'StatusLabel
        '
        Me.StatusLabel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.StatusLabel.AutoEllipsis = True
        Me.StatusLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.StatusLabel.Location = New System.Drawing.Point(24, 569)
        Me.StatusLabel.Name = "StatusLabel"
        Me.StatusLabel.Size = New System.Drawing.Size(804, 24)
        Me.StatusLabel.TabIndex = 3
        Me.StatusLabel.Text = "Ready"
        Me.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'UpdateTimer
        '
        Me.UpdateTimer.Interval = 90
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(17, Byte), Integer), CType(CType(23, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(850, 601)
        Me.Controls.Add(Me.ForksInput)
        Me.Controls.Add(Me.TokenButton)
        Me.Controls.Add(Me.Subtitle)
        Me.Controls.Add(Me.RepositoryButton)
        Me.Controls.Add(Me.OptionsPanel)
        Me.Controls.Add(Me.ProgressPanel)
        Me.Controls.Add(Me.ActivityPanel)
        Me.Controls.Add(Me.StatusLabel)
        Me.DoubleBuffered = True
        Me.Font = New System.Drawing.Font("Segoe UI", 10.5!)
        Me.ForeColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(243, Byte), Integer))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(792, 627)
        Me.Name = "Form1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "GitHub Repo Cloner 1.0.0"
        Me.OptionsPanel.ResumeLayout(False)
        Me.OptionsPanel.PerformLayout()
        CType(Me.DelayInput, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ProgressPanel.ResumeLayout(False)
        Me.ActivityPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Subtitle As Label
    Friend WithEvents RepositoryButton As UI.IconButton
    Friend WithEvents TokenButton As Button
    Friend WithEvents ForksInput As UI.ForkCheckBox
    Friend WithEvents RepositoryTip As ToolTip
    Friend WithEvents OptionsPanel As Panel
    Friend WithEvents OwnerLabel As Label
    Friend WithEvents OwnerInput As TextBox
    Friend WithEvents DestinationLabel As Label
    Friend WithEvents DestinationInput As TextBox
    Friend WithEvents BrowseButton As Button
    Friend WithEvents DelayLabel As Label
    Friend WithEvents DelayInput As NumericUpDown
    Friend WithEvents StartButton As Button
    Friend WithEvents CancelRunButton As Button
    Friend WithEvents ProgressPanel As Panel
    Friend WithEvents BatchHeading As Label
    Friend WithEvents BatchCounter As Label
    Friend WithEvents TotalCount As Label
    Friend WithEvents ClonedCount As Label
    Friend WithEvents SkippedCount As Label
    Friend WithEvents FailedCount As Label
    Friend WithEvents BatchBar As UI.ProgressTrack
    Friend WithEvents CurrentRepo As Label
    Friend WithEvents TransferStats As Label
    Friend WithEvents TransferBar As UI.ProgressTrack
    Friend WithEvents ActivityPanel As Panel
    Friend WithEvents ActivityLabel As Label
    Friend WithEvents ActivityLog As RichTextBox
    Friend WithEvents StatusLabel As Label
    Friend WithEvents UpdateTimer As Timer
End Class
