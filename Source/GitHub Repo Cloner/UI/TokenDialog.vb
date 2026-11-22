Option Strict On
Option Explicit On
Option Infer Off

''' <summary>Edits the locally saved token without displaying an existing credential.</summary>
Friend NotInheritable Class TokenDialog
    Inherits Form
    Private ReadOnly input As New TextBox()
    Private ReadOnly hint As New Label()
    Private ReadOnly saveButton As Button

    Public Sub New()
        Text = "GitHub API token"
        Font = New Font("Segoe UI", 10.0F)
        AutoScaleDimensions = New SizeF(96, 96)
        AutoScaleMode = AutoScaleMode.Dpi
        BackColor = Color.FromArgb(22, 27, 34)
        ForeColor = Color.Gainsboro
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(436, 158)
        hint.SetBounds(16, 10, 404, 46)
        input.SetBounds(16, 62, 404, 28)
        input.AccessibleName = "New GitHub personal access token"
        input.BackColor = Color.FromArgb(13, 17, 23)
        input.ForeColor = ForeColor
        input.UseSystemPasswordChar = True
        Controls.AddRange({hint, input})
        Dim removeButton As Button = MakeButton("Remove saved token", 16, 150)
        saveButton = MakeButton("Save", 240, 86)
        Dim dismiss As Button = MakeButton("Cancel", 334, 86)
        dismiss.DialogResult = DialogResult.Cancel
        AcceptButton = saveButton
        CancelButton = dismiss
        saveButton.Enabled = False
        ActiveControl = input
        AddHandler input.TextChanged, Sub() saveButton.Enabled = input.TextLength > 0
        AddHandler saveButton.Click, Sub() UpdateToken(False)
        AddHandler removeButton.Click, Sub() UpdateToken(True)
        Try
            Dim saved As Boolean = TokenStore.Current.HasSaved()
            removeButton.Enabled = saved
            hint.Text = If(saved, "A token is saved. Enter a new token to replace it.", If(String.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_TOKEN")), "Optional. Paste a personal access token for repository discovery.", "Using GITHUB_TOKEN from Windows. Saving a token here overrides it."))
        Catch ex As Exception
            hint.Text = "The saved token could not be read. You can replace or remove it."
        End Try
    End Sub

    Private Function MakeButton(caption As String, x As Integer, width As Integer) As Button
        Dim button As New Button With {.Text = caption, .FlatStyle = FlatStyle.Flat, .BackColor = Color.FromArgb(33, 38, 45)}
        button.FlatAppearance.BorderColor = Color.FromArgb(69, 78, 89)
        button.SetBounds(x, 108, width, 34)
        Controls.Add(button)
        Return button
    End Function

    Private Sub UpdateToken(remove As Boolean)
        Try
            If remove Then
                TokenStore.Current.Remove()
            Else
                TokenStore.Current.Save(input.Text)
            End If
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End Try
        input.Clear()
        DialogResult = DialogResult.OK
    End Sub
End Class
