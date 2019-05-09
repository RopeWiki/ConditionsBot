Imports HtmlAgilityPack
Imports System.Text.RegularExpressions
Imports System.Net

Module CanditionParsing
    Public Function GetAllCanditions() As List(Of Candition)
        Dim canditions As New List(Of Candition)
        Dim canyons As List(Of CanditionCanyon) = GetAllCanditionCanyons()
        For Each canyon As CanditionCanyon In canyons
            If canyon.LastDate Is Nothing Then Continue For
            canditions.AddRange(GetCanyonCanditions("http://www.candition.com" & canyon.Href))
        Next
        Return canditions
    End Function

    Public Class CanditionCanyon
        Public CanyonName As String
        Public Href As String
        Public Area As String
        Public LastDate As Date?

        Public Sub New(name As String, area As String, lastdate As String, href As String)
            Dim trim As New Regex("(?s)(?:^\s*|\s*$)")
            Me.CanyonName = trim.Replace(name, "").Replace("&amp;", "&")
            Me.Area = trim.Replace(area, "")
            Me.Href = trim.Replace(href, "")
            lastdate = trim.Replace(lastdate, "")
            If lastdate.Length > 0 Then
                Me.LastDate = Date.Parse(lastdate)
            Else
                Me.LastDate = Nothing
            End If
        End Sub
    End Class

    Public Function GetAllCanditionCanyons() As List(Of CanditionCanyon)
        Dim canyons As New List(Of CanditionCanyon)

        Dim wc As New WebClient()
        Using r = wc.OpenRead("http://www.candition.com/")
            Dim doc As New HtmlDocument()
            doc.Load(r)

            Dim areanodes As HtmlNodeCollection = doc.DocumentNode.SelectNodes("//div[@class='list'][child::div[@class='well canyon']]")
            If areanodes Is Nothing Then Throw New Exception("Could not find any area (div[@class='list'] with child div[@class='well canyon']) blocks on Candition.com index page")
            For Each a As HtmlNode In areanodes
                Dim areatext As String = a.Attributes("id").Value
                If areatext = "recent-canditions" Then Continue For
                Dim canyonnodes As HtmlNodeCollection = a.SelectNodes("./div[@class='well canyon']/div[@class='canyon_text']")
                If canyonnodes Is Nothing Then Throw New Exception("Couldn't find any canyons in area " & areatext)
                For Each c As HtmlNode In canyonnodes
                    Dim namenode As HtmlNode = c.SelectSingleNode("./h1/a")
                    If namenode Is Nothing Then Throw New Exception("Couldn't find name node for canyon in " & areatext)
                    Dim datenode As HtmlNode = c.SelectSingleNode("./h2/span[@class='user_date']")
                    If datenode Is Nothing Then
                        canyons.Add(New CanditionCanyon(namenode.InnerText, areatext, "", namenode.Attributes("href").Value))
                    Else
                        canyons.Add(New CanditionCanyon(namenode.InnerText, areatext, datenode.InnerText, namenode.Attributes("href").Value))
                    End If
                Next
            Next
        End Using

        Return canyons
    End Function

    Public Function MatchCanditionNamesToRopeWiki(canditions As List(Of CanditionCanyon), rw As RopeWiki) As Dictionary(Of CanditionCanyon, String)
        Dim matched As New Dictionary(Of CanditionCanyon, String)

        For Each cc As CanditionCanyon In canditions
            Dim ccn As String = Trim(LCase(cc.CanyonName))
            For Each c As String In rw.Canyons
                If Trim(LCase(c)) = ccn Then
                    matched(cc) = c
                    Exit For
                End If
            Next
            If matched.ContainsKey(cc) Then Continue For

            Dim ccncanyon As String = ccn & " canyon"
            For Each c As String In rw.Canyons
                If Trim(LCase(c)) = ccncanyon Then
                    matched(cc) = c
                    Exit For
                End If
            Next
            If matched.ContainsKey(cc) Then Continue For

            Dim ccnshort As String = Regex.Match(ccn, "(.*)\s\w+$").Groups(1).Value
            If ccnshort.Length > 0 Then
                Dim matches As New List(Of String)
                For Each c As String In rw.Canyons
                    If Trim(LCase(c)) = ccnshort Then matches.Add(c)
                Next
                If matches.Count = 1 Then matched(cc) = matches.First
            End If
            If matched.ContainsKey(cc) Then Continue For
        Next

        Return matched
    End Function

    Public Function GetLatestCanditions() As List(Of Candition)
        Console.WriteLine("Reading Candition index")

        Dim canditions As New List(Of Candition)

        Dim wc As New WebClient()
        Using r = wc.OpenRead("http://www.candition.com/")
            Dim doc As New HtmlDocument()
            doc.Load(r)

            Dim recent As HtmlNodeCollection = doc.DocumentNode.SelectNodes("//div[@id='recent-canditions']/div[@class='well canyon']/div[@class='canyon_text']/h1/a")
            If recent Is Nothing Then Throw New Exception("Could not find canyon_text blocks for recent-canditions on Candition.com index page")
            For Each c As HtmlNode In recent
                If c.Attributes("href") Is Nothing Then Throw New Exception("Anchor link in canyon_text block on Candition.com index page does not have an href attribute")
                canditions.AddRange(GetCanyonCanditions("http://www.candition.com" & c.Attributes("href").Value))
            Next
        End Using

        Return canditions
    End Function

    Private Function GetCanyonCanditions(url As String) As List(Of Candition)
        Console.WriteLine("  Reading Canditions for " & url)

        Dim canditions As New List(Of Candition)

        Dim wc As New WebClient
        Using r = wc.OpenRead(url)
            Dim doc As New HtmlDocument
            doc.Load(r)

            'Determine the canyon's name
            Dim canyonnamenode As HtmlNode = doc.DocumentNode.SelectSingleNode("//div[@class='well canyon_single']/div[@class='canyon_text']/h1")
            If canyonnamenode Is Nothing Then Throw New Exception("Couldn't find canyon name h1 in " & url)
            Dim canyonname As String = canyonnamenode.InnerText

            Dim entries As HtmlNodeCollection = doc.DocumentNode.SelectNodes("//div[@class='candition_box']")
            If entries Is Nothing Then Throw New Exception("Could not find any candition_box canditions on Candition at " & url)
            For Each c As HtmlNode In entries
                canditions.Add(ParseCandition(c, canyonname, url))
            Next
        End Using

        Console.WriteLine("    Parsed " & canditions.Count & " Canditions.")
        Return canditions
    End Function

    Private Function ParseCandition(n As HtmlNode, canyonname As String, url As String) As Candition
        Dim candition As New Candition

        candition.Canyon = canyonname
        candition.Url = url

        Dim contributornode As HtmlNode = n.SelectSingleNode("./div[@class='candition_user']/a[not(.//img)]")
        If contributornode Is Nothing Then Throw New Exception("Couldn't find candition_user div")
        candition.Contributor = contributornode.InnerText

        Dim ratingnode As HtmlNode = n.SelectSingleNode("./div[@class='candition_rating']")
        If ratingnode Is Nothing Then Throw New Exception("Couldn't find candition_rating div")
        Dim qualitymatch As Match = Regex.Match(ratingnode.InnerText, "(\d) out of 5")
        If Not qualitymatch.Success Then Throw New Exception("Couldn't find 'X out of 5' text in candition_rating div")
        candition.Quality = qualitymatch.Groups(1).Value

        Dim datenode As HtmlNode = ratingnode.SelectSingleNode("./span[@class='user_date']")
        If datenode Is Nothing Then Throw New Exception("Couldn't find user_date div within candition_rating div")
        candition.Timestamp = Date.Parse(datenode.InnerText)

        Dim statusnode As HtmlNode = n.SelectSingleNode("./div[@class='candition_status']")
        If statusnode Is Nothing Then Throw New Exception("Couldn't find candition_status div")
        candition.Description = Regex.Replace(statusnode.InnerText, "(?s)(?:^\s*|\s*$)", "")

        Return candition
    End Function

    Public Sub RenameFromHashToAuthor()
        Dim rw As RopeWiki = RopeWikiSessions.Session("BulkChangeRobot")
        Dim conditions As List(Of RopeWikiCondition) = ReadExportedConditions("rwconditions.xml")
        Dim findoldcanditionname As New Text.RegularExpressions.Regex("Candition\d+ [\dabcdef]{6}$")
        Dim findsubmitter As New Text.RegularExpressions.Regex("Submitted by \[[^ \]]* ([^\]]+)\]")
        For Each c As RopeWikiCondition In conditions
            If findoldcanditionname.IsMatch(c.PageName) Then
                Dim submitter As String = findsubmitter.Match(c.Comments).Groups(1).Value
                Dim newname As String = c.PageName.Substring(0, c.PageName.Length - 7) & " " & submitter
                Dim p As New DotNetWikiBot.Page(rw, c.PageName)
                p.Load()
                If p.Exists Then
                    If p.text.Contains("#REDIRECT") Then
                        p.Delete("Original condition page names no longer needed")
                    Else
                        p.RenameTo(newname, "Changed checksum page identifier to submitter")
                    End If
                End If
            End If
        Next
    End Sub
End Module
