Option Strict On
Option Explicit On
Option Infer Off

Namespace UI

    ''' <summary>A dark progress track with a themed fill.</summary>
    Public NotInheritable Class ProgressTrack
        Inherits Control

        Private fraction As Single

        Public Sub New()
            Me.DoubleBuffered = True
            Me.ResizeRedraw = True
            Me.AccessibleRole = AccessibleRole.ProgressBar
            Me.TabStop = False
        End Sub

        Public Sub UpdateProgress(value As Integer, maximum As Integer)
            Dim nextFraction As Single = If(maximum > 0, CSng(Math.Min(1.0, Math.Max(0.0, value / CDbl(maximum)))), 0.0F)
            If fraction = nextFraction Then Return
            fraction = nextFraction
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim width As Integer = CInt(Math.Floor(Me.ClientSize.Width * fraction))
            If width > 0 AndAlso Me.ClientSize.Height > 0 Then
                Using brush As New SolidBrush(Me.ForeColor)
                    e.Graphics.FillRectangle(brush, New Rectangle(0, 0, width, Me.ClientSize.Height))
                End Using
            End If
        End Sub
    End Class
End Namespace
