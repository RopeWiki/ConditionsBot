Imports System.Xml

Module RopeWikiParsing
    Public Function ReadExportedConditions(xmlexportfile As String) As List(Of RopeWikiCondition)
        Dim doc As New XmlDocument()
        doc.LoadXml(StripNamespace(xmlexportfile))

        Dim textnodes As XmlNodeList = doc.SelectNodes("//page")
        Dim result As New List(Of RopeWikiCondition)
        For Each n As XmlNode In textnodes
            result.Add(New RopeWikiCondition(n.SelectSingleNode("./title").InnerText, n.SelectSingleNode("./revision/text").InnerText))
        Next

        Return result
    End Function


End Module
