Imports DotNetWikiBot
Imports System.Text.RegularExpressions

Public Class RopeWiki
    Inherits Site

    Private _QualityConditions As Dictionary(Of Integer, String)
    Public ReadOnly Property QualityConditions As Dictionary(Of Integer, String)
        Get
            If _QualityConditions Is Nothing Then
                _QualityConditions = New Dictionary(Of Integer, String)
                Dim p As New Page(Me, "Property:Has condition quality")
                p.Load()
                If Not p.Exists Then Throw New Exception("'Property:Has condition quality' does not exist on RopeWiki")
                Dim mc As MatchCollection = Regex.Matches(p.text, "\[\[Allows value::([^\]]+)\]\]")
                Dim valuesplitter As New Regex("^\s*(\d+)\s*-\s*(.*)?$")
                For Each m As Match In mc
                    Dim kv As Match = valuesplitter.Match(m.Groups(1).Value)
                    If Not kv.Success Then Throw New Exception("Couldn't parse quality condition '" & m.Groups(1).Value & "'")
                    _QualityConditions(kv.Groups(1).Value) = m.Groups(1).Value
                Next
            End If

            Return _QualityConditions
        End Get
    End Property

    Private _Canyons As List(Of String)
    Public ReadOnly Property Canyons As List(Of String)
        Get
            If _Canyons Is Nothing Then
                Dim pl As New PageList(Me)
                pl.FillFromCategory("Canyons")
                _Canyons = New List(Of String)
                For Each p As Page In pl
                    _Canyons.Add(p.title)
                Next
            End If
            Return _Canyons
        End Get
    End Property

    Public Sub New(username As String, password As String)
        MyBase.New("http://ropewiki.com", username, password)
    End Sub
End Class
