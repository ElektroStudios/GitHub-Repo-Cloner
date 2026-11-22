Option Strict On
Option Explicit On
Option Infer Off

Imports GitHub_Repo_Cloner.Core

Public NotInheritable Class GitFormatTests

    <Theory>
    <InlineData("Receiving objects:  52% (75/144), 14.50 MiB | 3.75 MiB/s", "Receiving objects", 52, "14.50 MiB", "3.75 MiB/s")>
    <InlineData("Receiving objects: 100% (7/7), 596 bytes, done.", "Receiving objects", 100, "596 bytes", "")>
    <InlineData("remote: Compressing objects:  68% (2/3)", "Compressing objects", 68, "", "")>
    <InlineData("Resolving deltas:   0% (0/19)", "Resolving deltas", 0, "", "")>
    <InlineData("Updating files: 100% (63/63), done.", "Updating files", 100, "", "")>
    Public Sub ParserReadsTransferAndCheckoutPhases(line As String, phase As String, percent As Integer, size As String, rate As String)
        Dim transfer As GitTransfer = GitProgressParser.Parse(line)
        Assert.NotNull(transfer)
        Assert.Equal(phase, transfer.Phase)
        Assert.Equal(percent, transfer.Percent)
        Assert.Equal(size, transfer.Size)
        Assert.Equal(rate, transfer.Rate)
    End Sub

    <Fact>
    Public Sub FinalTransferSizeIsReportedEvenWhenGitAlreadyPrintedOneHundredPercent()
        Dim first As GitTransfer = GitProgressParser.Parse("Receiving objects: 100% (3/3)")
        Dim final As GitTransfer = GitProgressParser.Parse("Receiving objects: 100% (3/3), 128.24 KiB | 128.24 MiB/s, done.")
        Assert.True(GitProgressParser.ShouldReport(first, final, 1))
        Assert.False(GitProgressParser.ShouldReport(final, final, 1))
    End Sub

    <Theory>
    <InlineData("fatal: could not resolve host")>
    <InlineData("Receiving objects: 101%")>
    <InlineData("Receiving objects: -1%")>
    <InlineData("A message with Receiving objects: 50% text in it")>
    Public Sub ParserRejectsNonProgress(line As String)
        Assert.Null(GitProgressParser.Parse(line))
    End Sub

    <Theory>
    <InlineData("simple", """simple""")>
    <InlineData("path with spaces", """path with spaces""")>
    <InlineData("", """""")>
    Public Sub NativeArgumentsAreQuoted(value As String, expected As String)
        Assert.Equal(expected, GitClient.QuoteArgument(value))
    End Sub

    <Fact>
    Public Sub NativeArgumentQuotesEscapeEmbeddedQuotesAndTrailingSlashes()
        Dim slash As Char = ChrW(92)
        Dim quote As Char = """"c
        Assert.Equal($"{quote}C:{slash}space{slash}{slash}{quote}", GitClient.QuoteArgument($"C:{slash}space{slash}"))
        Assert.Equal($"{quote}A{slash}{quote}B{quote}", GitClient.QuoteArgument($"A{quote}B"))
    End Sub

    <Theory>
    <InlineData("..")>
    <InlineData("repo.")>
    <InlineData("foo/bar")>
    <InlineData("NUL.txt")>
    <InlineData(".git")>
    <InlineData("name:other")>
    Public Sub DestinationNamesCannotEscapeOrAliasSpecialFiles(name As String)
        Assert.False(CloneBatch.IsSafeName(name))
    End Sub

    <Theory>
    <InlineData("A")>
    <InlineData("@ElektroStudios")>
    <InlineData("example-account")>
    Public Sub AcceptsGitHubOwnerSyntax(value As String)
        Assert.Equal(value.TrimStart("@"c), CloneOptions.NormalizeOwner(value))
    End Sub

    <Theory>
    <InlineData("my--org")>
    <InlineData("@")>
    <InlineData("-abc")>
    <InlineData("a-")>
    <InlineData("abc/other")>
    <InlineData("abc?per_page=100")>
    <InlineData(" user")>
    <InlineData("user ")>
    <InlineData("user name")>
    <InlineData("@ user")>
    <InlineData("user" & vbTab)>
    <InlineData("user ")>
    Public Sub RejectsMalformedOwnerNames(value As String)
        Assert.Throws(Of ArgumentException)(Function() CloneOptions.NormalizeOwner(value))
    End Sub
End Class
