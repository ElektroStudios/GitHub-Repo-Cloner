Option Strict On
Option Explicit On
Option Infer Off

Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>Reads the progress format produced by Git with the C locale.</summary>
Friend NotInheritable Class GitProgressParser

    Private Shared ReadOnly ProgressLine As New Regex(
        "\A\s*(?:remote:\s*)?(?<phase>Receiving objects|Resolving deltas|Updating files|Checking out files|Enumerating objects|Counting objects|Compressing objects):\s*(?<percent>[0-9]{1,3})%",
        RegexOptions.CultureInvariant)
    Private Shared ReadOnly TransferSize As New Regex(
        ",\s*(?<size>[0-9]+(?:\.[0-9]+)?\s*(?:bytes?|[KMGT]i?B))(?:\s*\|\s*(?<rate>[0-9]+(?:\.[0-9]+)?\s*(?:bytes?|[KMGT]i?B)/s))?",
        RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)

    Private Sub New()
    End Sub

    Friend Shared Function ShouldReport(previous As GitTransfer, current As GitTransfer, elapsedMilliseconds As Long) As Boolean
        Return previous Is Nothing OrElse previous.Phase <> current.Phase OrElse
               (previous.Size.Length = 0 AndAlso current.Size.Length > 0) OrElse
               (current.Percent = 100 AndAlso (previous.Percent <> 100 OrElse previous.Size <> current.Size OrElse previous.Rate <> current.Rate)) OrElse
               elapsedMilliseconds >= 125
    End Function

    Friend Shared Function Parse(line As String) As GitTransfer
        Dim progress As Match = ProgressLine.Match(line)
        If Not progress.Success Then Return Nothing
        Dim percent As Integer = Integer.Parse(progress.Groups("percent").Value, CultureInfo.InvariantCulture)
        If percent > 100 Then Return Nothing
        Dim phase As String = progress.Groups("phase").Value
        Dim transfer As Match = TransferSize.Match(line, progress.Length)
        Return New GitTransfer(phase, percent, transfer.Groups("size").Value, transfer.Groups("rate").Value)
    End Function
End Class
