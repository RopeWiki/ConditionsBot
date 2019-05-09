Imports System.Text.RegularExpressions

Public Class RopeWikiCondition
    Public PageName As String
    Public Location As String = ""
    Public ReportedBy As String = ""
    Public Timestamp As Date
    Public QualityCondition As String = ""
    Public WaterCondition As String = ""
    Public WetsuitCondition As String = ""
    Public DifficultyCondition As String = ""
    Public TeamTime As String = ""
    Public TeamSize As String = ""
    Public TeamMinExperience As String = ""
    Public TeamMaxExperience As String = ""
    Public Comments As String = ""
    Public Url As String = ""

    Public Sub New(c As Candition, botname As String, rw As RopeWiki)
        Me.PageName = c.RopeWikiPageTitle(rw)
        Me.Location = c.RopeWikiName(rw)
        Me.ReportedBy = botname
        Me.Timestamp = c.Timestamp
        If Not rw.QualityConditions.ContainsKey(c.Quality) Then Throw New Exception("Unrecognized Candition quality: " & c.Quality)
        Me.QualityCondition = rw.QualityConditions(c.Quality)
        Me.Comments = "See the [" & c.Url & " full condition report at Candition.com].  Submitted by [" & c.UserUrl & " " & c.Contributor & "]."
        Me.Url = c.Url
    End Sub

    Private Shared PARAMFINDER As New Regex("\|([^\|]*)=([^\|]*)")
    Public Sub New(title As String, wikitext As String)
        If Not wikitext.Contains("{{Condition") Then Throw New Exception("wikitext does not contain a Condition template")
        Me.PageName = title
        Dim parammatches As MatchCollection = PARAMFINDER.Matches(wikitext)
        For Each pm As Match In parammatches
            Dim key As String = pm.Groups(1).Value
            Dim value As String = pm.Groups(2).Value
            If value.EndsWith("}}") AndAlso Not value.Contains("{{") Then value = Left(value, value.Length - 2)
            Select Case key
                Case "Location"
                    Me.Location = value
                Case "ReportedBy"
                    Me.ReportedBy = value
                Case "Date"
                    Me.Timestamp = Date.Parse(value)
                Case "Quality condition"
                    Me.QualityCondition = value
                Case "Water condition"
                    Me.WaterCondition = value
                Case "Wetsuit condition"
                    Me.WetsuitCondition = value
                Case "Difficulty condition"
                    Me.DifficultyCondition = value
                Case "Team time"
                    Me.TeamTime = value
                Case "Team size"
                    Me.TeamSize = value
                Case "Team min experience"
                    Me.TeamMinExperience = value
                Case "Team max experience"
                    Me.TeamMaxExperience = value
                Case "Url"
                    Me.Url = value
                Case "Comments"
                    Me.Comments = value
            End Select
        Next
    End Sub

    Public Function AsWikiText() As String
        Dim sb As New Text.StringBuilder
        sb.AppendLine("{{Condition")
        sb.AppendLine("|Location=" & Me.Location)
        sb.AppendLine("|ReportedBy=" & Me.ReportedBy)
        sb.AppendLine("|Date=" & Me.Timestamp.ToString("yyyy/MM/dd"))
        sb.AppendLine("|Quality condition=" & Me.QualityCondition)
        sb.AppendLine("|Water condition=" & Me.WaterCondition)
        sb.AppendLine("|Wetsuit condition=" & Me.WetsuitCondition)
        sb.AppendLine("|Difficulty condition=" & Me.DifficultyCondition)
        sb.AppendLine("|Team time=" & Me.TeamTime)
        sb.AppendLine("|Team size=" & Me.TeamSize)
        sb.AppendLine("|Team min experience=" & Me.TeamMinExperience)
        sb.AppendLine("|Team max experience=" & Me.TeamMaxExperience)
        sb.AppendLine("|Url=" & Me.Url)
        sb.AppendLine("|Comments=" & Me.Comments)
        sb.Append("}}")
        Return sb.ToString
    End Function
End Class
