Imports System.IO

Public Class WebService
    Private ServiceUri As Uri
    Public Sub New(URI As String)
        Me.ServiceUri = New Uri(URI)
    End Sub

    Public Function getData(Data As String) As WebServiceResult
        Return New WebServiceResult(Me.ServiceUri, Data)
    End Function
End Class

Public Class Element
    Public Name As String
    Public Childs As List(Of Integer)
    Public Text As String
    Public Parent As Integer
End Class

Public Class WebServiceResult
    Public Elements As Dictionary(Of Integer, Element)
    Public Sub New(ServiceUri As Uri, Data As String)
        Elements = New Dictionary(Of Integer, Element)
        Dim webClient As New System.Net.WebClient
        Dim uri As String = ServiceUri.AbsoluteUri + "/xml/" + Data
        Dim result As String = webClient.DownloadString(uri)
        Dim XMLData As New System.Xml.XmlTextReader(New StringReader(result))
        Dim cElement As Integer = 0
        Dim mElement As Integer = 0

        While XMLData.Read()
            Select Case XMLData.NodeType
                Case Xml.XmlNodeType.Element
                    Dim e As New Element
                    e.Name = XMLData.Name
                    Dim name As String = XMLData.Name
                    e.Parent = cElement
                    e.Childs = New List(Of Integer)

                    If (cElement > 0) Then
                        Elements.Item(cElement).Childs.Add(mElement + 1)
                    End If
                    mElement = mElement + 1

                    If (Not XMLData.IsEmptyElement) Then
                        cElement = mElement
                    End If

                    Elements.Add(mElement, e)
                Case Xml.XmlNodeType.EndElement
                    If (cElement > 0) Then
                        If Elements.Item(cElement).Name = XMLData.Name Then
                            Dim parent As Integer = Elements.Item(cElement).Parent
                            cElement = parent
                        End If
                    End If
                Case Xml.XmlNodeType.Text
                    Elements.Item(cElement).Text = XMLData.Value
            End Select
        End While
    End Sub
    Public Function getElement(Name As String, Optional elementIndex As Integer = 1) As Integer
        If (Me.Elements.ContainsKey(elementIndex)) Then
            For Each c As Integer In Me.Elements.Item(elementIndex).Childs
                If (Me.Elements.Item(c).Name = Name) Then
                    Return c
                End If
            Next
        End If
        Return 0
    End Function
End Class
