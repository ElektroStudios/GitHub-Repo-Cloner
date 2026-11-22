Option Strict On
Option Explicit On
Option Infer Off

Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>Inputs validated before a batch can run.</summary>
Public NotInheritable Class CloneOptions

    Private Shared ReadOnly OwnerPattern As New Regex("\A[A-Za-z0-9](?:[A-Za-z0-9]|-(?=[A-Za-z0-9])){0,38}\z", RegexOptions.CultureInvariant)

    Public ReadOnly Property Owner As String
    Public ReadOnly Property DirectoryPath As String
    Public ReadOnly Property Delay As TimeSpan

    Public ReadOnly Property IncludeForks As Boolean

    Public Sub New(owner As String, directoryPath As String, delay As TimeSpan, Optional includeForks As Boolean = False)
        Me.IncludeForks = includeForks
        Me.Owner = NormalizeOwner(owner)
        If String.IsNullOrWhiteSpace(directoryPath) Then Throw New ArgumentException("Enter a destination directory.", NameOf(directoryPath))
        If delay < TimeSpan.Zero OrElse delay > TimeSpan.FromHours(1) Then
            Throw New ArgumentOutOfRangeException(NameOf(delay), "The delay must be between zero and one hour.")
        End If

        Me.DirectoryPath = Path.GetFullPath(directoryPath.Trim())
        Me.Delay = delay
    End Sub

    Friend Shared Function NormalizeOwner(owner As String) As String
        If owner Is Nothing Then Throw New ArgumentNullException(NameOf(owner))
        Dim normalized As String = owner
        If normalized.StartsWith("@", StringComparison.Ordinal) Then normalized = normalized.Substring(1)
        If Not OwnerPattern.IsMatch(normalized) Then
            Throw New ArgumentException("Enter a valid GitHub username without spaces. The @ prefix is optional.", NameOf(owner))
        End If
        Return normalized
    End Function

End Class

''' <summary>A public repository listed by GitHub.</summary>
Public NotInheritable Class Repository
    Public ReadOnly Property Name As String
    Public ReadOnly Property IsFork As Boolean

    Public Sub New(name As String, isFork As Boolean)
        If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("A repository name is required.", NameOf(name))
        Me.Name = name
        Me.IsFork = isFork
    End Sub
End Class

Public Interface IRepositoryCatalog
    Function GetRepositoriesAsync(owner As String, report As Action(Of String), cancellation As CancellationToken) As Task(Of IReadOnlyList(Of Repository))
End Interface

Public Interface IGitClient
    Function CheckAsync(cancellation As CancellationToken) As Task
    Function CloneAsync(repositoryUri As Uri, target As String, report As Action(Of GitTransfer), cancellation As CancellationToken) As Task
End Interface

''' <summary>A reported Git phase and its latest transfer information.</summary>
Public NotInheritable Class GitTransfer
    Public ReadOnly Property Phase As String
    Public ReadOnly Property Percent As Integer
    Public ReadOnly Property Size As String
    Public ReadOnly Property Rate As String

    Public Sub New(phase As String, percent As Integer, size As String, rate As String)
        Me.Phase = phase
        Me.Percent = percent
        Me.Size = size
        Me.Rate = rate
    End Sub
End Class

Public Enum BatchMessageKind
    Information
    Success
    Warning
    Failure
End Enum

''' <summary>A snapshot of the batch counts and the current repository.</summary>
Public NotInheritable Class BatchUpdate
    Public ReadOnly Property Total As Integer
    Public ReadOnly Property Cloned As Integer
    Public ReadOnly Property Skipped As Integer
    Public ReadOnly Property Failed As Integer
    Public ReadOnly Property Processed As Integer
        Get
            Return Cloned + Skipped + Failed
        End Get
    End Property
    Public ReadOnly Property RepositoryName As String
    Public ReadOnly Property Transfer As GitTransfer
    Public ReadOnly Property Message As String
    Public ReadOnly Property Kind As BatchMessageKind

    Friend Sub New(total As Integer, cloned As Integer, skipped As Integer, failed As Integer,
                   repositoryName As String, transfer As GitTransfer, message As String, kind As BatchMessageKind)
        Me.Total = total
        Me.Cloned = cloned
        Me.Skipped = skipped
        Me.Failed = failed
        Me.RepositoryName = repositoryName
        Me.Transfer = transfer
        Me.Message = message
        Me.Kind = kind
    End Sub
End Class

Public NotInheritable Class GitCommandException
    Inherits IOException

    Public ReadOnly Property IsTransient As Boolean
    Public Sub New(message As String, isTransient As Boolean)
        MyBase.New(message)
        Me.IsTransient = isTransient
    End Sub
End Class
