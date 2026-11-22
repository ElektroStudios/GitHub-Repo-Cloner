Option Strict On
Option Explicit On
Option Infer Off

#If NET48 Then
Imports System.Reflection
Imports GitHub_Repo_Cloner.Tests.UI

Public NotInheritable Class ForkCheckBoxTests
    <Theory>
    <InlineData(False, True)>
    <InlineData(True, True)>
    <InlineData(True, False)>
    Public Sub PaintingOverAReusedBufferErasesPreviousPixels(isChecked As Boolean, isEnabled As Boolean)
        Using control As New ForkCheckBox With {.Size = New Size(140, 34), .BackColor = Color.FromArgb(13, 17, 23),
                .Text = "Include forks", .Checked = isChecked, .Enabled = isEnabled},
              bitmap As New Bitmap(control.Width, control.Height), surface As Graphics = Graphics.FromImage(bitmap)
            surface.Clear(Color.Magenta)
            TextRenderer.DrawText(surface, "Stale caption", control.Font, New Point(7, 1), Color.Orange)
            Dim paint As MethodInfo = GetType(ForkCheckBox).GetMethod("OnPaint", BindingFlags.Instance Or BindingFlags.NonPublic)
            Using args As New PaintEventArgs(surface, control.ClientRectangle)
                paint.Invoke(control, New Object() {args})
            End Using
            Assert.Equal(control.BackColor.ToArgb(), bitmap.GetPixel(0, 0).ToArgb())
            For y As Integer = 0 To bitmap.Height - 1
                For x As Integer = 0 To bitmap.Width - 1
                    Assert.NotEqual(Color.Magenta.ToArgb(), bitmap.GetPixel(x, y).ToArgb())
                    Assert.NotEqual(Color.Orange.ToArgb(), bitmap.GetPixel(x, y).ToArgb())
                Next
            Next
        End Using
    End Sub
End Class
#End If
