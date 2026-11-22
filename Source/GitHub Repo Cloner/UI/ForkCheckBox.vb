Option Strict On
Option Explicit On
Option Infer Off

Imports System.Drawing.Drawing2D

Namespace UI
    ''' <summary>A keyboard-accessible checkbox with a fully painted dark surface.</summary>
    Public NotInheritable Class ForkCheckBox
        Inherits CheckBox

        Public Sub New()
            SetStyle(ControlStyles.Opaque Or ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
            AutoSize = False
            FlatStyle = FlatStyle.Flat
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            'A reused WinForms paint buffer can contain pixels from another control.
            Using background As New SolidBrush(BackColor)
                e.Graphics.FillRectangle(background, e.ClipRectangle)
            End Using

            Dim size As Integer = Math.Min(Height - 4, Font.Height)
            If size < 4 Then Return
            Dim box As New Rectangle(3, (Height - size) \ 2, size, size)
            Dim ink As Color = If(Enabled, Color.Gainsboro, Color.Gray)
            Dim accent As Color = If(Enabled AndAlso Checked, Color.FromArgb(54, 99, 146), Color.FromArgb(33, 38, 45))
            Dim hovered As Boolean = Enabled AndAlso ClientRectangle.Contains(PointToClient(Cursor.Position))
            Using fill As New SolidBrush(accent), border As New Pen(If(hovered OrElse Focused, Color.LightSteelBlue, Color.SlateGray))
                e.Graphics.FillRectangle(fill, box)
                e.Graphics.DrawRectangle(border, box)
            End Using
            If Checked Then
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
                Using mark As New Pen(ink, CSng(size) / 8)
                    e.Graphics.DrawLines(mark, {New Point(box.X + size \ 5, box.Y + size \ 2), New Point(box.X + size * 2 \ 5, box.Y + size * 7 \ 10), New Point(box.X + size * 4 \ 5, box.Y + size \ 4)})
                End Using
            End If
            Dim labelArea As Rectangle = Rectangle.FromLTRB(box.Right + 7, 0, Width, Height)
            TextRenderer.DrawText(e.Graphics, Text, Font, labelArea, ink, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix Or TextFormatFlags.PreserveGraphicsClipping)
            If Focused AndAlso ShowFocusCues Then ControlPaint.DrawFocusRectangle(e.Graphics, labelArea, ink, BackColor)
        End Sub
    End Class
End Namespace
