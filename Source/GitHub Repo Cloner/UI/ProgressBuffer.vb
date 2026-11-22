Option Strict On
Option Explicit On
Option Infer Off

Imports System.Collections.Concurrent
Imports GitHub_Repo_Cloner.Core

Namespace UI

    ''' <summary>Passes worker notifications to the next UI refresh without posting per Git progress line.</summary>
    Friend NotInheritable Class ProgressBuffer
        Implements IProgress(Of BatchUpdate)

        Private ReadOnly pending As New ConcurrentQueue(Of BatchUpdate)()

        Public ReadOnly Property HasPending As Boolean
            Get
                Return Not pending.IsEmpty
            End Get
        End Property

        Public Sub Report(value As BatchUpdate) Implements IProgress(Of BatchUpdate).Report
            pending.Enqueue(value)
        End Sub

        Public Function TryDequeue(ByRef value As BatchUpdate) As Boolean
            Return pending.TryDequeue(value)
        End Function

    End Class
End Namespace
