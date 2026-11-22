Option Strict On
Option Explicit On
Option Infer Off

Imports Microsoft.Win32
Imports System.Security.Cryptography
Imports System.Text
Imports GitHub_Repo_Cloner.Core

''' <summary>Stores the optional API credential in the current user's registry hive.</summary>
Friend NotInheritable Class TokenStore
    Friend Shared ReadOnly Current As New TokenStore( "Software\ElektroStudios\GitHub Repo Cloner")
    Private Const ValueName As String = "ApiToken"

    Private ReadOnly keyPath As String

    Friend Sub New(keyPath As String)
        If String.IsNullOrWhiteSpace(keyPath) Then Throw New ArgumentException("A registry key path is required.", NameOf(keyPath))
        Me.keyPath = keyPath
    End Sub

    Friend Function HasSaved() As Boolean
        Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(keyPath)
            Return key IsNot Nothing AndAlso Array.IndexOf(key.GetValueNames(), ValueName) >= 0
        End Using
    End Function

    Friend Function ReadSaved() As String
        Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(KeyPath)
            If key Is Nothing Then Return Nothing
            Dim value As Object = key.GetValue(ValueName, Nothing, RegistryValueOptions.DoNotExpandEnvironmentNames)
            If value Is Nothing Then Return Nothing
            Dim bytes As Byte() = TryCast(value, Byte())
            If bytes Is Nothing Then Throw New InvalidOperationException("The saved token is unreadable. Replace or remove it in API token settings.")
            Try
                Dim plain As Byte() = ProtectedData.Unprotect(bytes, Nothing, DataProtectionScope.CurrentUser)
                Try
                    Dim token As String = Encoding.UTF8.GetString(plain)
                    GitHubCatalog.ValidateToken(token)
                    Return token
                Finally
                    Array.Clear(plain, 0, plain.Length)
                End Try
            Catch ex As CryptographicException
                Throw New InvalidOperationException("Windows could not decrypt the saved token. Replace or remove it in API token settings.", ex)
            End Try
        End Using
    End Function

    Friend Sub Save(token As String)
        GitHubCatalog.ValidateToken(token)
        Dim plain As Byte() = Encoding.UTF8.GetBytes(token)
        Try
            Dim cipher As Byte() = ProtectedData.Protect(plain, Nothing, DataProtectionScope.CurrentUser)
            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey(KeyPath)
                key.SetValue(ValueName, cipher, RegistryValueKind.Binary)
            End Using
        Finally
            Array.Clear(plain, 0, plain.Length)
        End Try
    End Sub

    Friend Sub Remove()
        Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(KeyPath, writable:=True)
            If key IsNot Nothing Then key.DeleteValue(ValueName, throwOnMissingValue:=False)
        End Using
    End Sub
End Class
