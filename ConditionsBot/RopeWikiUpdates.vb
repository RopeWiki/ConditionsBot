Imports DotNetWikiBot
Imports System.Xml
Imports System.Net
Imports System.Text.RegularExpressions

Module RopeWikiUpdates
    Public Sub UpdateConditionQuality()
        Dim qualitymap As New Dictionary(Of String, String)

        qualitymap("") = ""
        qualitymap("0 - Unknown") = "0 - Unknown"
        qualitymap("1 - Not worth doing") = "1 - Poor"
        qualitymap("2 - Ok") = "2 - Ok"
        qualitymap("3 - Worthwhile") = "3 - Good"
        qualitymap("4 - Great") = "4 - Great"
        qualitymap("5 - Among the best") = "5 - Amazing"

        Dim rw As RopeWiki = RopeWikiSessions.Session("BulkChangeRobot")
        Dim reports As New PageList(rw)
        reports.FillFromCategory("Conditions")

        For Each p As Page In reports
            Dim savepage As Boolean = False
            p.Load()
            savepage = savepage Or UpdateTemplateArgument(p, "Condition", "Quality condition", qualitymap)
            If savepage Then
                p.ShowText()
                p.Save("Updated Condition template property values", True)
            End If
        Next
    End Sub

    Private Function UpdateTemplateArgument(p As Page, templatename As String, argumentname As String, valuemapping As Dictionary(Of String, String)) As Boolean
        Dim l As List(Of String) = p.GetTemplateParameter(templatename, argumentname)
        If l.Count = 0 Then
            Console.WriteLine("Couldn't find Condition::Quality condition in page " & p.title)
        ElseIf l.Count > 1 Then
            Console.WriteLine("Found multiple Condition::Quality condition in page " & p.title)
        Else
            If Not valuemapping.ContainsKey(l.First) Then Throw New Exception("Mapping did not contain the value '" & l.First & "'")
            If valuemapping(l.First) <> l.First Then
                p.SetTemplateParameter(templatename, argumentname, valuemapping(l.First), False)
                Return True
            End If
        End If
        Return False
    End Function

    Public Function RemoveCanyonQuality(istart As Integer) As Integer
        Dim rw As RopeWiki = RopeWikiSessions.Session("BulkChangeRobot")
        Dim canyons As New PageList(rw)
        canyons.FillFromCategory("Canyons")

        Dim i As Integer = 0
        For Each p As Page In canyons
            If i >= istart Then
                Dim savepage As Boolean = False
                p.Load()
                savepage = savepage Or RemoveTemplateArgument(p, "Canyon", "Quality rating")
                If savepage Then
                    Try
                        p.Save("Removed Quality rating from Canyon template", True)
                    Catch ex As Exception
                        If ex.Message.StartsWith("Insufficient rights to edit page") Then Return i
                        Throw New Exception("Error saving page " & p.title, ex)
                    End Try
                End If
            End If
            i += 1
        Next
        Return -1
    End Function

    Private Function RemoveTemplateArgument(p As Page, templatename As String, argumentname As String) As Boolean
        Dim l As List(Of String) = p.GetTemplateParameter(templatename, argumentname)
        If l.Count = 0 Then
            Console.WriteLine("Couldn't find " & templatename & "::" & argumentname & " in page " & p.title)
        ElseIf l.Count > 1 Then
            Console.WriteLine("Found multiple " & templatename & "::" & argumentname & " in page " & p.title)
        Else
            Return p.RemoveTemplateParameter(templatename, argumentname, False) > 0
        End If
        Return False
    End Function

    Public Sub UpdateCanditions(Optional all As Boolean = False)
        Dim canditions As List(Of Candition)
        If all Then canditions = GetAllCanditions() Else canditions = GetLatestCanditions()
        Dim rw As RopeWiki = RopeWikiSessions.Session("CanditionBot")
        Dim pages As List(Of Page) = MakePages(canditions, rw)
        RemoveExistingPages(pages, rw)
        While pages.Count > 0
            Dim p As Page = pages(0)
            Dim pexist As New Page(rw, p.title)
            pexist.text = p.text
            For i = 1 To 3
                Try
                    pexist.Save("Updated from Candition on " & Now.ToString("yyyy/MM/dd"), False)
                    Exit For
                Catch ex As WikiBotException
                    Console.WriteLine("Rate limited (" + ex.Message + "); waiting...")
                    Threading.Thread.Sleep(60000)
                    Console.WriteLine("Logging in and retrying...")
                    rw = RopeWikiSessions.Session("CanditionBot")
                End Try
            Next i
            pages.RemoveAt(0)
        End While
    End Sub

    Public Sub CreateInitialCanditions()
        Dim rw As RopeWiki = RopeWikiSessions.Session("CanditionBot")
        Dim canditions As List(Of Candition) = Candition.DeserializeFromFile("canditions.xml")
        Dim pages As List(Of Page) = MakePages(canditions, rw)
        Dim xml As XmlDocument = ExportPagesToXml(pages)
        xml.Save("initialcanditions.xml")
    End Sub

    Public Sub RemoveExistingPages(pages As List(Of Page), rw As RopeWiki)
        Dim i As Integer = 0
        While i < pages.Count
            Dim p As Page = pages(i)
            Dim pexist As New Page(rw, p.title)
            pexist.Load()
            If pexist.Exists Then
                pages.RemoveAt(i)
            Else
                i += 1
            End If
        End While
    End Sub

    Public Function MakePages(canditions As List(Of Candition), rw As RopeWiki) As List(Of Page)
        Dim conditions As New List(Of RopeWikiCondition)
        For Each c As Candition In canditions
            conditions.Add(New RopeWikiCondition(c, "CanditionBot", rw))
        Next
        Return MakePages(conditions)
    End Function

    Public Function MakePages(conditions As List(Of RopeWikiCondition)) As List(Of Page)
        Dim pages As New List(Of Page)
        For Each rwc As RopeWikiCondition In conditions
            If rwc.Location.Length = 0 Then Continue For
            Dim rwp As New Page(rwc.PageName)
            rwp.text = rwc.AsWikiText
            pages.Add(rwp)
        Next
        Return pages
    End Function

    Public Function ExportPagesToXml(pages As List(Of Page)) As XmlDocument
        Dim doc As New XmlDocument

        Dim mwnode As XmlNode = doc.CreateElement("mediawiki")
        doc.AppendChild(mwnode)

        For Each p As Page In pages
            Dim pnode As XmlNode = doc.CreateElement("page")
            mwnode.AppendChild(pnode)

            pnode.AppendChild(TextNode(doc, "title", p.title))

            Dim revnode As XmlNode = doc.CreateElement("revision")

            Dim contribnode As XmlNode = doc.CreateElement("contributor")
            contribnode.AppendChild(TextNode(doc, "username", "CanditionBot"))
            contribnode.AppendChild(TextNode(doc, "id", "558"))

            revnode.AppendChild(contribnode)
            revnode.AppendChild(TextNode(doc, "text", p.text))
            revnode.AppendChild(TextNode(doc, "comment", "Generated wiki content"))
            revnode.AppendChild(TextNode(doc, "model", "wikitext"))
            revnode.AppendChild(TextNode(doc, "format", "text/x-wiki"))

            pnode.AppendChild(revnode)
        Next

        Return doc
    End Function

    Private Function TextNode(doc As XmlDocument, element As String, content As String) As XmlNode
        Dim n As XmlNode = doc.CreateElement(element)
        n.AppendChild(doc.CreateTextNode(content))
        Return n
    End Function

    Public Sub FixRatingPageNames()
        Dim rw As RopeWiki = RopeWikiSessions.Session("RenameRobot")
        Dim editmsg As String = "Fixed rating page name mismatch"
        Console.WriteLine("Finding rating pages...")
        Dim querytext As String = "{{#ask: [[Has page rating page::+]] [[Has page rating user::+]] |format=list |limit=10000}}"
        Dim url As String = "http://ropewiki.com/api.php?action=parse&format=xml&text=" & System.Web.HttpUtility.UrlEncode(querytext)
        Dim doc As New XmlDocument
        Using client As New WebClient
            Dim xmltext As String = client.DownloadString(url)
            doc.LoadXml(xmltext)
        End Using
        Dim links As XmlNodeList = doc.SelectNodes("//api/parse/links/pl")
        Dim i As Integer = 0, j As Integer = 0, k As Integer = 0
        Console.WriteLine("Checking page names...")
        For Each conditionnode As XmlNode In links
            Dim ratingpagename As String = conditionnode.InnerText
            Dim p As New Page(rw, ratingpagename)
            p.Load()
            If Not p.Exists Then Throw New Exception("Linked rating page '" & ratingpagename & "' does not exist")
            Dim pagename As String = p.GetTemplateParameter("Page rating", "Page").First
            Dim username As String = p.GetTemplateParameter("Page rating", "User").First
            Dim newpagename As String = "Votes:" & pagename & "/" & username
            If newpagename <> ratingpagename Then
                Dim p2 As New Page(rw, newpagename)
                p2.Load()
                If p2.Exists Then
                    Console.WriteLine("DUPLICATE FOUND for " + newpagename)
                    k += 1
                Else
                    Console.WriteLine("Renaming '" + ratingpagename + "' to '" + newpagename + "'")
                    p.RenameTo(newpagename, editmsg)
                    j += 1
                End If
            Else
                Console.WriteLine(ratingpagename & " ok.")
            End If
            i += 1
        Next
        Console.WriteLine(i & " rating pages examined, " & j & " renames made, " & k & " duplicates ignored")
    End Sub

    Public Sub FixSectionHeadings()
        Dim rw As RopeWiki = RopeWikiSessions.Session("BulkChangeRobot")
        Dim editmsg As String = "Removed spaces from section headers to work with Edit with Form"
        Console.WriteLine("Finding Canyons...")
        Dim querytext As String = "{{#ask: [[Category:Canyons]] |format=list |limit=10000}}"
        Dim url As String = "http://ropewiki.com/api.php?action=parse&format=xml&text=" & System.Web.HttpUtility.UrlEncode(querytext)
        Dim doc As New XmlDocument
        Using client As New WebClient
            Dim xmltext As String = client.DownloadString(url)
            doc.LoadXml(xmltext)
        End Using
        Dim links As XmlNodeList = doc.SelectNodes("//api/parse/links/pl")
        Dim i As Integer = 0, j As Integer = 0, k As Integer = 0
        Dim headings As New List(Of String)
        headings.Add("Introduction")
        headings.Add("Approach")
        headings.Add("Descent")
        headings.Add("Exit")
        headings.Add("Red tape")
        headings.Add("Beta sites")
        headings.Add("Trip reports and media")
        headings.Add("Background")
        Console.WriteLine("Checking " & links.Count & " Canyons...")
        For Each canyonnode As XmlNode In links
            Do
                Dim canyonpagename As String = canyonnode.InnerText
                Dim p As New Page(rw, canyonpagename)
                p.Load()
                If Not p.Exists Then
                    Console.WriteLine("Canyon page '" & canyonpagename & "' does not exist")
                    Exit Do
                End If
                Dim change As Boolean = False
                For Each heading As String In headings
                    Dim m As Match = Regex.Match(p.text, "^( *)\=\=( *)" + heading + "( *)\=\=( *)$", RegexOptions.Multiline)
                    If m.Success AndAlso (m.Groups(1).Value.Length > 0 OrElse m.Groups(2).Value.Length > 0 OrElse m.Groups(3).Value.Length > 0 OrElse m.Groups(4).Value.Length > 0) Then
                        change = True
                        p.text = p.text.Substring(0, m.Index) & "==" & heading & "==" & p.text.Substring(m.Index + m.Length)
                    End If
                Next
                If change Then
                    j += 1
                    Console.WriteLine("Fixing " & canyonpagename)
                    Try
                        p.Save(editmsg, False)
                    Catch e As Exception
                        Console.WriteLine("ERROR SAVING: " & e.ToString())
                        rw = RopeWikiSessions.Session("BulkChangeRobot")
                        Continue Do
                    End Try
                Else
                    Console.WriteLine(canyonpagename + " ok")
                End If
                i += 1
                Exit Do
            Loop
        Next
        Console.WriteLine(i & " Canyons examined, " & j & " adjustments made")
    End Sub
End Module
