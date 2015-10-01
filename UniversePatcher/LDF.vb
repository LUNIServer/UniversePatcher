Imports System.IO

Public Class LDF
    Private File As String

    Public Sub New()
        File = ""
    End Sub

    Private Sub WritePair(Key As String, type As Integer, value As String)
        If (Not File = "") Then
            File = File.Insert(File.Length, ",")
        End If
        File = File.Insert(File.Length, Key & "=" & type & ":" & value)
    End Sub

    Public Sub WriteString(Key As String, value As String)
        WritePair(Key, 0, value)
    End Sub

    Public Sub WriteInteger(key As String, value As Int32)
        WritePair(key, 1, value.ToString)
    End Sub

    Public Sub WriteUnsignedInt(key As String, value As UInt32)
        WritePair(key, 5, value)
    End Sub

    Public Sub WriteBoolean(key As String, value As Boolean)
        If value Then
            WritePair(key, 7, "1")
        Else
            WritePair(key, 7, "0")
        End If
    End Sub

    Public Function getResult() As String
        Return File
    End Function
End Class
