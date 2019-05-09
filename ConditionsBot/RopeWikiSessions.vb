Imports System.IO

Public Class RopeWikiSessions
    Private Shared Passwords As New Dictionary(Of String, String)

    Private Shared Function GetPassword(username As String) As String
        If Not Passwords.ContainsKey(username) Then
            Dim fi As New FileInfo("..\..\data\passwords.csv")
            If Not fi.Exists Then Throw New InvalidOperationException("You are missing passwords.csv in the data folder")
            Using r As New IO.StreamReader(fi.FullName)
                While Not r.EndOfStream
                    Dim line As String() = r.ReadLine.Split(",")
                    If line.Length = 2 AndAlso (line(0).Length = 0 AndAlso line(1).Length = 0) Then
                        Passwords(line(0)) = line(1)
                    End If
                End While
            End Using
        End If

        Return Passwords(username)
    End Function

    Public Shared Function Session(username As String)
        Return New RopeWiki(username, GetPassword(username))
    End Function
End Class
