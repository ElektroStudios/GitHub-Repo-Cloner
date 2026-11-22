Option Strict On
Option Explicit On
Option Infer Off

Imports System.Globalization
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices

Namespace My
    Partial Friend Class MyApplication
        Private Sub ApplyEnglishCulture(sender As Object, e As StartupEventArgs) Handles Me.Startup
            Dim english As CultureInfo = CultureInfo.GetCultureInfo("en-US")
            CultureInfo.DefaultThreadCurrentCulture = english
            CultureInfo.DefaultThreadCurrentUICulture = english
            Thread.CurrentThread.CurrentCulture = english
            Thread.CurrentThread.CurrentUICulture = english
        End Sub
    End Class
End Namespace
