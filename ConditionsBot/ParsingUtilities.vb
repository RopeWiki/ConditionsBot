Imports System.Reflection
Imports System.IO
Imports System.Text.RegularExpressions

Module ParsingUtilities
    Public Function GetUniqueValues(Of T)(conditions As List(Of T)) As Dictionary(Of String, HashSet(Of String))
        Dim result As New Dictionary(Of String, HashSet(Of String))
        For Each fi As FieldInfo In GetType(T).GetFields
            Dim unique As New HashSet(Of String)
            For Each c As T In conditions
                Dim value As String = fi.GetValue(c)
                If value.Length > 0 AndAlso Not unique.Contains(value) Then unique.Add(value)
            Next
            result(fi.Name) = unique
        Next
        Return result
    End Function

    Public Function StripNamespace(xmlfilename As String) As String
        Dim xml As String
        Using r As New StreamReader(xmlfilename)
            xml = r.ReadToEnd
        End Using
        Return Regex.Replace(xml, "\s+xmlns=""[^""]*""", "")
    End Function
End Module
