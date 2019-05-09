Imports System.Net
Imports HtmlAgilityPack

Module CanyonCollectiveParsing
    Public Function GetAllCanyons()
        Dim canyons

        Dim wc As New WebClient
        Dim url As String = "http://canyoncollective.com/betabase/"
        Using r = wc.OpenRead(url)
            Dim doc As New HtmlDocument
            doc.Load(r)

            'Iterate over areas
            Dim areas As HtmlNodeCollection = doc.DocumentNode.SelectNodes("//div[@class='secondaryContent showcaseCategoryList']/ol/li/a")
            If areas Is Nothing Then Throw New Exception("Couldn't find areas list in " & url)
            For Each areanode As HtmlNode In areas
                canyons.add(GetAreaCanyons(areanode.Attributes("href").Value, areanode.InnerText))
            Next
        End Using

        Return canyons
    End Function

    Private Function GetAreaCanyons(href As String, areaname As String)

    End Function
End Module
