Option Strict On
Option Explicit On
Option Infer Off

Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

Namespace UI

    ''' <summary>An icon-only control with button semantics and fully custom painting.</summary>
    Public NotInheritable Class IconButton
        Inherits Control

        Private icon As Bitmap
        Private pointerInside As Boolean
        Private pointerPressed As Boolean
        Private pressedKey As Keys
        Private pointerFocus As Boolean

        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or ControlStyles.SupportsTransparentBackColor Or
                        ControlStyles.ResizeRedraw Or ControlStyles.Selectable, True)
            Me.SetStyle(ControlStyles.StandardClick Or ControlStyles.StandardDoubleClick, False)
            Me.BackColor = Color.Transparent
            Me.TabStop = True
            Me.AccessibleRole = AccessibleRole.PushButton
        End Sub

        ''' <summary>Copies a glyph into an owned bitmap and preserves its alpha channel.</summary>
        Public Sub SetIcon(image As Image)
            If image Is Nothing Then Throw New ArgumentNullException(NameOf(image))

            Dim bitmap As New Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb)
            Try
                Using graphics As Graphics = Graphics.FromImage(bitmap)
                    graphics.CompositingMode = CompositingMode.SourceCopy
                    Dim bounds As New Rectangle(0, 0, bitmap.Width, bitmap.Height)
                    graphics.DrawImage(image, bounds, bounds, GraphicsUnit.Pixel)
                End Using
            Catch
                bitmap.Dispose()
                Throw
            End Try

            If Me.icon IsNot Nothing Then Me.icon.Dispose()
            Me.icon = bitmap
            Me.Invalidate()
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing AndAlso Me.icon IsNot Nothing Then
                Me.icon.Dispose()
                Me.icon = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim diameter As Integer = Math.Min(Me.Width, Me.Height) - Me.LogicalToDeviceUnits(6)
            If diameter <= 0 Then Return

            Dim graphics As Graphics = e.Graphics
            Dim state As GraphicsState = graphics.Save()
            Try
                graphics.SmoothingMode = SmoothingMode.AntiAlias
                graphics.CompositingMode = CompositingMode.SourceOver
                Dim circle As New Rectangle((Me.Width - diameter) \ 2, (Me.Height - diameter) \ 2, diameter, diameter)
                Dim pressed As Boolean = (Me.pointerInside AndAlso Me.pointerPressed) OrElse Me.pressedKey <> Keys.None
                If Me.Enabled AndAlso (Me.pointerInside OrElse pressed) Then
                    Using fill As New SolidBrush(If(pressed, Color.FromArgb(48, 54, 61), Color.FromArgb(33, 38, 45)))
                        graphics.FillEllipse(fill, circle)
                    End Using
                End If

                If Me.icon IsNot Nothing Then
                    Me.PaintIcon(graphics, Math.Min(diameter, Me.LogicalToDeviceUnits(20)))
                End If

                If Me.Enabled AndAlso Me.Focused AndAlso Not Me.pointerFocus AndAlso Me.ShowFocusCues Then
                    Dim focusColor As Color = If(SystemInformation.HighContrast, SystemColors.Highlight, Color.FromArgb(88, 166, 255))
                    Using outline As New Pen(focusColor, Me.LogicalToDeviceUnits(2))
                        graphics.DrawEllipse(outline, circle)
                    End Using
                End If
            Finally
                graphics.Restore(state)
            End Try
        End Sub

        Private Sub PaintIcon(graphics As Graphics, size As Integer)
            Dim scale As Double = size / CDbl(Math.Max(Me.icon.Width, Me.icon.Height))
            Dim width As Integer = Math.Max(1, CInt(Math.Round(Me.icon.Width * scale)))
            Dim height As Integer = Math.Max(1, CInt(Math.Round(Me.icon.Height * scale)))
            Dim target As New Rectangle((Me.Width - width) \ 2, (Me.Height - height) \ 2, width, height)

            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic
            graphics.PixelOffsetMode = PixelOffsetMode.Half
            Using attributes As New ImageAttributes()
                attributes.SetWrapMode(WrapMode.TileFlipXY)
                If Not Me.Enabled Then
                    attributes.SetColorMatrix(New ColorMatrix() With {.Matrix33 = 0.45F})
                End If
                graphics.DrawImage(Me.icon, target, 0, 0, Me.icon.Width, Me.icon.Height, GraphicsUnit.Pixel, attributes)
            End Using
        End Sub

        Protected Overrides Sub OnMouseEnter(e As EventArgs)
            MyBase.OnMouseEnter(e)
            Me.pointerInside = True
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            MyBase.OnMouseLeave(e)
            Me.pointerInside = False
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            If e.Button <> MouseButtons.Left OrElse Not Me.Enabled Then Return

            Me.pointerFocus = True
            Me.Focus()
            Me.pointerPressed = True
            Me.Capture = True
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
            MyBase.OnMouseUp(e)
            If e.Button <> MouseButtons.Left Then Return

            Dim activate As Boolean = Me.pointerPressed AndAlso Me.ClientRectangle.Contains(e.Location)
            Me.pointerPressed = False
            Me.Capture = False
            Me.Invalidate()
            If activate Then Me.PerformClick()
        End Sub

        Protected Overrides Sub OnMouseCaptureChanged(e As EventArgs)
            MyBase.OnMouseCaptureChanged(e)
            If Not Me.Capture Then
                Me.pointerPressed = False
                Me.Invalidate()
            End If
        End Sub

        Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
            Dim key As Keys = keyData And Keys.KeyCode
            Return key = Keys.Space OrElse key = Keys.Enter OrElse MyBase.IsInputKey(keyData)
        End Function

        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Modifiers <> Keys.None OrElse (e.KeyCode <> Keys.Space AndAlso e.KeyCode <> Keys.Enter) Then Return

            If Me.pressedKey = Keys.None Then Me.pressedKey = e.KeyCode
            Me.pointerFocus = False
            e.Handled = True
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
            MyBase.OnKeyUp(e)
            If e.KeyCode <> Me.pressedKey Then Return

            Me.pressedKey = Keys.None
            e.Handled = True
            Me.Invalidate()
            If Me.Focused Then Me.PerformClick()
        End Sub

        Protected Overrides Sub OnGotFocus(e As EventArgs)
            MyBase.OnGotFocus(e)
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnLostFocus(e As EventArgs)
            Me.pointerFocus = False
            Me.pointerPressed = False
            Me.pressedKey = Keys.None
            MyBase.OnLostFocus(e)
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnChangeUICues(e As UICuesEventArgs)
            MyBase.OnChangeUICues(e)
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            Me.pointerPressed = False
            Me.pressedKey = Keys.None
            MyBase.OnEnabledChanged(e)
            Me.Invalidate()
        End Sub

        Protected Overrides Function CreateAccessibilityInstance() As AccessibleObject
            Return New IconButtonAccessibleObject(Me)
        End Function

        Private Sub PerformClick()
            If Me.Enabled AndAlso Me.Visible Then Me.OnClick(EventArgs.Empty)
        End Sub

        Private NotInheritable Class IconButtonAccessibleObject
            Inherits ControlAccessibleObject

            Private ReadOnly button As IconButton

            Public Sub New(button As IconButton)
                MyBase.New(button)
                Me.button = button
            End Sub

            Public Overrides ReadOnly Property DefaultAction As String
                Get
                    Return "Press"
                End Get
            End Property

            Public Overrides Sub DoDefaultAction()
                Me.button.PerformClick()
            End Sub
        End Class

    End Class
End Namespace
