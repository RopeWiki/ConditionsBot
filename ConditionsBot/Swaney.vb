Imports System.IO
Imports DotNetWikiBot
Imports System.Text
Imports System.Text.RegularExpressions

Module Swaney
    Private Class SwaneyCanyon
        Public Index As Integer
        Public SwaneyName As String
        Public RwName As String
        Public Explored As DateTime?
        Public SwaneyIndex As String
        Public Coordinates As String
        Public Region As String

        Public Sub New(csv As String)
            Dim cols As New List(Of String)
            Dim value As String = ""
            Dim inquotes = False
            For Each c As Char In csv
                If inquotes Then
                    If c = """" Then inquotes = False Else value &= c
                ElseIf c = """" Then
                    inquotes = True
                ElseIf c = "," Then
                    cols.Add(value)
                    value = ""
                Else
                    value &= c
                End If
            Next
            cols.Add(value)
            Index = cols(0)
            SwaneyName = cols(1)
            RwName = cols(2)
            If cols(3).Length > 0 Then Explored = DateTime.Parse(cols(3)) Else Explored = Nothing
            SwaneyIndex = cols(4)
            Region = cols(7)
        End Sub

        Public Function MakePage() As String
            Dim sb As New StringBuilder
            sb.AppendLine("{{Canyon")
            sb.AppendLine("|Technical rating=")
            sb.AppendLine("|Water rating=")
            sb.AppendLine("|Time rating=")
            sb.AppendLine("|Extra risk rating=")
            sb.AppendLine("|Fastest typical time=")
            sb.AppendLine("|Slowest typical time=")
            sb.AppendLine("|Hike length=")
            sb.AppendLine("|Approach elevation gain=")
            sb.AppendLine("|Exit elevation gain=")
            sb.AppendLine("|Number of rappels=")
            sb.AppendLine("|Longest rappel=")
            sb.AppendLine("|Region=Death Valley National Park")
            sb.AppendLine("|Coordinates=" & Coordinates)
            sb.AppendLine("|Vehicle=")
            sb.AppendLine("|Shuttle=")
            sb.AppendLine("|Permits=No")
            sb.AppendLine("|Explored=Yes")
            sb.AppendLine("|Topo map=")
            sb.AppendLine("|Explored by=Scott Swaney")
            sb.AppendLine("}}")
            sb.AppendLine("== Introduction ==")
            sb.AppendLine("== Approach ==")
            sb.AppendLine("== Descent ==")
            sb.AppendLine("== Exit ==")
            sb.AppendLine("== Red tape ==")
            sb.AppendLine("== Beta sites ==")
            sb.AppendLine("== Trip reports and media ==")
            sb.AppendLine("== Background ==")
            If Explored.HasValue Then sb.AppendLine("Originally explored by [[User:Scott Swaney|Scott Swaney]] on " & Explored.Value.ToString("yyyy-MM-dd") & " under the identifier """ & SwaneyIndex & """.  See more at [[Scott Swaney's Death Valley Canyoneering Exploration]].")
            Return sb.ToString()
        End Function
    End Class

    Private Function ReadCanyons() As Dictionary(Of String, SwaneyCanyon)
        Dim canyons As New Dictionary(Of String, SwaneyCanyon)
        Using r = New StreamReader("C:\Users\Ben\Documents\Canyoneering\SwaneyNames.csv")
            Dim l As String = r.ReadLine
            While l IsNot Nothing
                Dim sc As New SwaneyCanyon(l)
                canyons(sc.SwaneyName) = sc
                l = r.ReadLine
            End While
        End Using
        Return canyons
    End Function

    Public Sub AddCanyons()
        Dim canyons As Dictionary(Of String, SwaneyCanyon) = ReadCanyons

        Dim kml As String
        Using r = New StreamReader("C:\Archive\Recovery20140713\Users\Ben\Places\Scott_Swaney's_Death_Valley_Canyons (1).kml")
            kml = r.ReadToEnd
        End Using
        Dim pathfinder As New Regex("(?s)<Placemark>.*?<name>([^<]*)</name>.*?<coordinates>\s*([^<]*)\s*</coordinates>.*?</Placemark>") '\s*\s*
        Dim mc As MatchCollection = pathfinder.Matches(kml)
        For Each m As Match In mc
            If canyons.ContainsKey(m.Groups(1).Value) Then
                Dim ptstrings As String() = Regex.Split(m.Groups(2).Value, "\s+")
                Dim center As String() = ptstrings(Math.Floor(ptstrings.Length / 2)).Split(",")
                canyons(m.Groups(1).Value).Coordinates = center(1) & ", " & center(0)
            End If
        Next

        Dim rw As RopeWiki = RopeWikiSessions.Session("Scott Swaney")
        For Each c As SwaneyCanyon In canyons.Values
            Dim p As New Page(rw, c.RwName)
            p.Load()
            If p.Exists Then
                Continue For
            End If

            p.text = c.MakePage()

            p.Save("Added explored canyon stub", False)
        Next
    End Sub

    Public Function CanyonList() As String
        Dim canyons As Dictionary(Of String, SwaneyCanyon) = ReadCanyons()

        Dim names As New List(Of String)
        For Each c As SwaneyCanyon In canyons.Values
            names.Add(c.RwName)
        Next
        names.Sort()

        Dim sb As New StringBuilder
        For Each n As String In names
            sb.AppendLine("* [[" & n & "]]")
        Next
        Return sb.ToString
    End Function

    Public Sub SetRegions()
        Dim canyons As Dictionary(Of String, SwaneyCanyon) = ReadCanyons()

        Dim rw As RopeWiki = RopeWikiSessions.Session("Bjp")

        For Each c As SwaneyCanyon In canyons.Values
            Dim p As New Page(rw, c.RwName)
            p.Load()
            Dim before As String = p.text
            p.SetTemplateParameter("Canyon", "Region", c.Region, True)
            If p.text <> before Then p.Save("Made region specific", True)
        Next
    End Sub

    Public Sub AddPropertyTags()
        Dim canyons As Dictionary(Of String, SwaneyCanyon) = ReadCanyons()

        Dim rw As RopeWiki = RopeWikiSessions.Session("Scott Swaney")

        For Each c As SwaneyCanyon In canyons.Values
            Dim p As New Page(rw, c.RwName)
            p.Load()
            If Not p.text.StartsWith("{{#set") Then
                p.text = "{{#set:Is Swaney exploration=true}}" & vbCrLf & p.text
                p.Save("Added property tag for Swaney exploration", True)
            End If
        Next
    End Sub
End Module
