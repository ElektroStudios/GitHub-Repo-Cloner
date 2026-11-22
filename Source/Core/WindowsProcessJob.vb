Option Strict On
Option Explicit On
Option Infer Off

#If NETFRAMEWORK Then
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports Microsoft.Win32.SafeHandles

''' <summary>Keeps Git's Windows subprocesses in a kill-on-close job when the host permits it.</summary>
Friend NotInheritable Class WindowsProcessJob

    Private Const ExtendedLimitInformation As Integer = 9
    Private Const KillOnJobClose As UInteger = &H2000UI

    Private Sub New()
    End Sub

    Friend Shared Function TryAttach(process As Process) As SafeFileHandle
        Dim job As SafeFileHandle = CreateJobObject(IntPtr.Zero, Nothing)
        If job.IsInvalid Then
            job.Dispose()
            Return Nothing
        End If

        Dim configuration As New ExtendedLimits()
        configuration.Basic.LimitFlags = KillOnJobClose
        Dim attached As Boolean = False
        Try
            If SetInformationJobObject(job, ExtendedLimitInformation, configuration, CUInt(Marshal.SizeOf(GetType(ExtendedLimits)))) AndAlso
               Not process.HasExited Then
                attached = AssignProcessToJobObject(job, process.Handle)
            End If
            Return If(attached, job, Nothing)
        Finally
            If Not attached Then job.Dispose()
        End Try
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure BasicLimits
        Public PerProcessUserTimeLimit As Long
        Public PerJobUserTimeLimit As Long
        Public LimitFlags As UInteger
        Public MinimumWorkingSetSize As UIntPtr
        Public MaximumWorkingSetSize As UIntPtr
        Public ActiveProcessLimit As UInteger
        Public Affinity As UIntPtr
        Public PriorityClass As UInteger
        Public SchedulingClass As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure IoCounters
        Public ReadOperationCount As ULong
        Public WriteOperationCount As ULong
        Public OtherOperationCount As ULong
        Public ReadTransferCount As ULong
        Public WriteTransferCount As ULong
        Public OtherTransferCount As ULong
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure ExtendedLimits
        Public Basic As BasicLimits
        Public Io As IoCounters
        Public ProcessMemoryLimit As UIntPtr
        Public JobMemoryLimit As UIntPtr
        Public PeakProcessMemoryUsed As UIntPtr
        Public PeakJobMemoryUsed As UIntPtr
    End Structure

    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Shared Function CreateJobObject(attributes As IntPtr, name As String) As SafeFileHandle
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function SetInformationJobObject(job As SafeFileHandle, informationClass As Integer, ByRef information As ExtendedLimits, length As UInteger) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function AssignProcessToJobObject(job As SafeFileHandle, process As IntPtr) As Boolean
    End Function

End Class
#End If
