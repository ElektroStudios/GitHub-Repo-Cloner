Option Strict On
Option Explicit On
Option Infer Off

Imports GitHub_Repo_Cloner.Core
#If NET48 Then
Imports Microsoft.Win32
Imports System.Text
#End If

Public NotInheritable Class TokenTests
    <Theory>
    <InlineData("")>
    <InlineData(" ")>
    <InlineData(" token")>
    <InlineData("token ")>
    <InlineData("abc def")>
    Public Sub RejectsMalformedTokenBeforeSaving(value As String)
        Assert.Throws(Of ArgumentException)(Sub() GitHubCatalog.ValidateToken(value))
    End Sub

#If NET48 Then
    <Fact>
    Public Sub WindowsCredentialCanBeReplacedRecoveredAndRemoved()
        Dim path As String = $"Software\ElektroStudios\ClonerTests-{Guid.NewGuid():N}"
        Dim store As New TokenStore(path)
        Try
            Assert.Null(store.ReadSaved())
            store.Save("first-example")
            Assert.True(store.HasSaved())
            Assert.Equal("first-example", New TokenStore(path).ReadSaved())
            Assert.Throws(Of ArgumentException)(Sub() store.Save("invalid token"))
            Assert.Equal("first-example", store.ReadSaved())
            store.Save("next-example")
            Assert.Equal("next-example", store.ReadSaved())
            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(path, True)
                Dim cipher As Byte() = DirectCast(key.GetValue("ApiToken"), Byte())
                Assert.DoesNotContain("next-example", Encoding.UTF8.GetString(cipher))
                key.SetValue("ApiToken", New Byte() {0, 1}, RegistryValueKind.Binary)
            End Using
            Assert.Throws(Of InvalidOperationException)(Function() store.ReadSaved())
            store.Remove()
            store.Remove()
            Assert.False(store.HasSaved())
            Assert.Null(store.ReadSaved())
        Finally
            Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey:=False)
        End Try
    End Sub
#End If
End Class
