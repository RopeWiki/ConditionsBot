Imports System.Security.Cryptography
Imports System.Xml
Imports System.IO

Public Class Candition
    Public Canyon As String
    Public Timestamp As Date
    Public Contributor As String
    Public Quality As String
    Public Description As String
    Public Url As String

    Private _CanditionMap As Dictionary(Of String, String)
    Private ReadOnly Property CanditionMap As Dictionary(Of String, String)
        Get
            If _CanditionMap Is Nothing Then
                _CanditionMap = New Dictionary(Of String, String)
                Dim fi As New FileInfo("..\..\data\canditionnamemap.csv")
                If Not fi.Exists Then fi = New FileInfo("canditionnamemap.csv")
                Using r As New IO.StreamReader(fi.FullName)
                    While Not r.EndOfStream
                        Dim line As String() = SplitLine(r.ReadLine)
                        If line.Length = 3 AndAlso (line(2).Length = 0 OrElse line(2)(0) <> "<") Then
                            _CanditionMap(line(0)) = line(2)
                        End If
                    End While
                End Using
            End If
            Return _CanditionMap
        End Get
    End Property

    Private Function SplitLine(line As String) As String()
        Dim parts As New List(Of String)
        Dim currentpart As New Text.StringBuilder
        Dim inquotes As Boolean = False
        line = line.Replace("""""", "<###QUOTE###>")
        For Each c As Char In line
            If c = """" Then
                inquotes = Not inquotes
            ElseIf c = "," AndAlso inquotes = False Then
                parts.Add(currentpart.ToString.Replace("<###QUOTE###>", """"))
                currentpart = New Text.StringBuilder
            Else
                currentpart.Append(c)
            End If
        Next
        parts.Add(currentpart.ToString)
        Return parts.ToArray
    End Function

    Public ReadOnly Property UserUrl As String
        Get
            Return "http://www.candition.com/profile/" & Contributor
        End Get
    End Property

    Public ReadOnly Property ContentId As String
        Get
            Dim sb As New Text.StringBuilder
            sb.AppendLine(Me.Canyon)
            sb.AppendLine(Me.Timestamp.ToString())
            sb.AppendLine(Me.Contributor)
            sb.AppendLine(Me.Quality)
            sb.AppendLine(Me.Description)
            sb.AppendLine(Me.Url)

            Dim hash As Byte()
            Using MD5 As MD5 = MD5.Create()
                hash = MD5.ComputeHash(Text.Encoding.UTF8.GetBytes(sb.ToString))
            End Using

            sb.Clear()
            For Each h As Byte In hash
                sb.Append(h.ToString("x2"))
            Next

            Return sb.ToString
        End Get
    End Property

    Public Function RopeWikiPageTitle(rw As RopeWiki) As String
        Return "Conditions:" & Me.RopeWikiName(rw) & "-Candition" & Me.Timestamp.ToString("yyyyMMdd_") & Me.Contributor
    End Function

    Public Function RopeWikiName(rw As RopeWiki) As String
        If CanditionMap.ContainsKey(Me.Canyon) Then Return CanditionMap(Me.Canyon)

        Dim ccn As String = Trim(LCase(Me.Canyon))
        For Each c As String In rw.Canyons
            If Trim(LCase(c)) = ccn Then Return c
        Next

        Dim comments As String = Me.Description.ToLower
        If ccn = "imlay" Then
            If comments.Contains("sneak route") OrElse comments.Contains("left sneak") OrElse comments.Contains("right sneak") OrElse comments.Contains("imlay sneak") Then
                Return "Imlay Canyon (Sneak Route)"
            ElseIf comments.Contains("full") Then
                Return "Imlay Canyon (Full)"
            End If
        End If

        Dim matches As New List(Of String)
        For Each c As String In rw.Canyons
            If Trim(LCase(c)).Contains(ccn) Then matches.Add(c)
        Next
        If matches.Count = 1 Then Return matches.First

        If matches.Count = 0 Then
            matches.Clear()
            For Each c As String In rw.Canyons
                If ccn.Contains(Trim(LCase(c))) Then matches.Add(c)
            Next
            If matches.Count = 1 Then Return matches.First
        End If

        If matches.Count = 0 Then
            Return ""
        End If

        Dim msg As String = matches.Count & " possible matches found for Candition canyon " & Me.Canyon
        For Each m As String In matches
            msg &= vbCrLf & "  " & m
        Next
        Return ""
    End Function

    Private Sub SerializeTo(w As XmlWriter)
        w.WriteStartElement("Candition")
        w.WriteAttributeString("Canyon", Me.Canyon)
        w.WriteAttributeString("Timestamp", Me.Timestamp.ToString("yyyy-MM-dd"))
        w.WriteAttributeString("Contributor", Me.Contributor)
        w.WriteAttributeString("Quality", Me.Quality)
        w.WriteAttributeString("Url", Me.Url)
        w.WriteStartElement("Description")
        w.WriteString(Me.Description)
        w.WriteEndElement()
        w.WriteEndElement()
    End Sub

    Public Shared Sub SerializeToFile(canditions As List(Of Candition), filename As String)
        Using w As New XmlTextWriter(filename, Text.Encoding.UTF8)
            w.Formatting = Xml.Formatting.Indented
            w.Indentation = 2

            w.WriteStartDocument()
            w.WriteStartElement("CanditionList")
            For Each c As Candition In canditions
                c.SerializeTo(w)
            Next
            w.WriteEndElement()
            w.WriteEndDocument()
        End Using
    End Sub

    Public Shared Function DeserializeFromFile(filename As String) As List(Of Candition)
        Dim doc As New XmlDocument()
        doc.Load(filename)

        Dim canditions As New List(Of Candition)
        Dim nodes As XmlNodeList = doc.DocumentElement.SelectNodes("//Candition")
        For Each n As XmlNode In nodes
            Dim c As New Candition
            c.Canyon = n.Attributes("Canyon").Value
            c.Timestamp = Date.Parse(n.Attributes("Timestamp").Value)
            c.Contributor = n.Attributes("Contributor").Value
            c.Quality = n.Attributes("Quality").Value
            c.Url = n.Attributes("Url").Value
            c.Description = n.SelectSingleNode("./Description").InnerText
            canditions.Add(c)
        Next

        Return canditions
    End Function
End Class
