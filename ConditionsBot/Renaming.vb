Imports DotNetWikiBot
Imports System.Net
Imports System.Xml
Imports System.Text.RegularExpressions

Module Renaming
    Public Sub RenameCanyon(ByVal oldname As String, ByVal newname As String)
        Dim rw As RopeWiki = RopeWikiSessions.Session("RenameRobot")
        Dim editmsg As String = "Modified to support canyon name change from " & oldname & " to " & newname

        Console.WriteLine("Renaming root page...")
        Dim p As New Page(rw, oldname)
        p.Load()
        Try
            p.RenameTo(newname, editmsg)
        Catch
            Console.WriteLine("Old canyon page '" & oldname & "' does not exist")
        End Try

        Console.WriteLine("Renaming talk page...")
        p = New Page(rw, "Talk:" & oldname)
        p.Load()
        Try
            p.RenameTo("Talk:" & newname, editmsg)
        Catch ex As Exception
            Console.WriteLine("Talk page didn't exist (no rename)")
        End Try

        Console.WriteLine("Renaming banner image...")
        p = New Page(rw, "File:" & oldname & " Banner.jpg")
        p.Load()
        Try
            p.RenameTo("File:" & newname & " Banner.jpg", editmsg)
        Catch
            Console.WriteLine("Banner image didn't exist (no rename)")
        End Try

        Console.WriteLine("Renaming KML map...")
        p = New Page(rw, "File:" & oldname & ".kml")
        p.Load()
        'If p.Exists Then
        Try
            p.RenameTo("File:" & newname & ".kml", editmsg)
        Catch
            Console.WriteLine("KML map didn't exist (no rename)")
        End Try
        'End If

        'Update rating references
        Console.WriteLine("Finding ratings...")
        Dim querytext As String = "{{#ask: [[Has page rating::+]] [[Has page rating page::" & oldname & "]] |format=list}}"
        Dim url As String = "http://ropewiki.com/api.php?action=parse&format=xml&text=" & System.Web.HttpUtility.UrlEncode(querytext)
        Dim doc As New XmlDocument
        Using client As New WebClient
            Dim xmltext As String = client.DownloadString(url)
            doc.LoadXml(xmltext)
        End Using
        Dim links As XmlNodeList = doc.SelectNodes("//api/parse/links/pl")
        Console.WriteLine("Updating rating pages...")
        For Each ratingnode As XmlNode In links
            Dim ratingpagename As String = ratingnode.InnerText
            p = New Page(rw, ratingpagename)
            p.Load()
            If Not p.Exists Then Throw New Exception("Linked rating page '" & ratingpagename & "' does not exist")
            p.SetTemplateParameter("Page rating", "Page", newname, False)
            p.Save(editmsg, False)
            p.RenameTo(ratingpagename.Replace(oldname, newname), editmsg)
        Next

        'Update conditions
        Console.WriteLine("Finding conditions...")
        querytext = "{{#ask: [[Has condition location::" & oldname & "]] |format=list}}"
        url = "http://ropewiki.com/api.php?action=parse&format=xml&text=" & System.Web.HttpUtility.UrlEncode(querytext)
        doc = New XmlDocument
        Using client As New WebClient
            Dim xmltext As String = client.DownloadString(url)
            doc.LoadXml(xmltext)
        End Using
        links = doc.SelectNodes("//api/parse/links/pl")
        Console.WriteLine("Updating condition references...")
        For Each conditionnode As XmlNode In links
            Dim conditionpagename As String = conditionnode.InnerText
            p = New Page(rw, conditionpagename)
            p.Load()
            If Not p.Exists Then Throw New Exception("Linked condition page '" & conditionpagename & "' does not exist")
            p.SetTemplateParameter("Condition", "Location", newname, False)
            p.Save(editmsg, False)
        Next

        'Update incidents
        Console.WriteLine("Finding incidents...")
        querytext = "{{#ask: [[Has incident location::" & oldname & "]] |format=list}}"
        url = "http://ropewiki.com/api.php?action=parse&format=xml&text=" & System.Web.HttpUtility.UrlEncode(querytext)
        doc = New XmlDocument
        Using client As New WebClient
            Dim xmltext As String = client.DownloadString(url)
            doc.LoadXml(xmltext)
        End Using
        links = doc.SelectNodes("//api/parse/links/pl")
        Console.WriteLine("Updating incident references...")
        For Each incidentnode As XmlNode In links
            Dim incidentpagename As String = incidentnode.InnerText
            p = New Page(rw, incidentpagename)
            p.Load()
            If Not p.Exists Then Throw New Exception("Linked incident page '" & incidentpagename & "' does not exist")
            p.SetTemplateParameter("Incident", "Location", newname, False)
            p.Save(editmsg, False)
        Next

        'Update {{pic}} images
        Console.WriteLine("Finding {{pic}} images...")
        url = "http://ropewiki.com/index.php?title=" & System.Web.HttpUtility.UrlEncode(newname) & "&action=raw"
        Dim pagecontent As String
        Using client As New WebClient
            pagecontent = client.DownloadString(url)
        End Using
        Dim picFinder As New Regex("\{\{pic\|(.*?)\}\}")
        Console.WriteLine("Updating {{pic}} file locations...")
        For Each m As Match In picFinder.Matches(pagecontent)
            Dim picParams As String = m.Groups(1).Value
            Dim picArgs As String() = picParams.Split(";").Select(Function(s) s.Trim()).ToArray()
            For Each picArg As String In picArgs
                If picArg.Contains(":") Then Continue For
                Dim fileSuffix As String = picArg
                If fileSuffix.Contains("~") Then fileSuffix = fileSuffix.Substring(0, fileSuffix.IndexOf("~")).Trim()
                Dim srcPicName As String = "File:" & oldname & "_" & fileSuffix
                Dim destPicName As String = "File:" & newname & "_" & fileSuffix
                p = New Page(rw, srcPicName)
                p.Load()
                p.RenameTo(destPicName, editmsg)
            Next
        Next
    End Sub

    Public Sub FixVotePageNames()

    End Sub
End Module
