Imports Awesomium.Core
Imports System.IO

Public Class Form1
    Dim LauncherFrameObject As JSObject
    Dim LauncherUniverseButton As JSObject

    Dim launcherURL As String

    Dim ConfigUrl As String
    Dim StatusUrl As String

    Dim clientURL As String

    Dim patcherUrl As Uri

    Dim Env As WebServiceResult = Nothing
    Dim ServerList As New List(Of ServerInfo)
    Dim Suggested As Integer = 0
    Dim Selected As Integer = 0

    Dim FRAME As JSObject

    Public Structure ServerInfo
        Public Name As String
        Public Address As String
        Public Suggested As Boolean
    End Structure

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim INI As StreamReader = My.Computer.FileSystem.OpenTextFileReader(My.Computer.FileSystem.CurrentDirectory + "\lunipatcher.ini")
        Dim line As String = ""

        clientURL = "..\client"
        Dim serverURL As String = "http://localhost/UniverseConfig/UniverseConfig.svc"

        While Not INI.EndOfStream
            line = INI.ReadLine()
            If (line.Length > 10) Then
                If (line.StartsWith("ConfigUrl=")) Then
                    serverURL = line.Substring(10)
                End If
                If (line.StartsWith("ClientUrl=")) Then
                    clientURL = line.Substring(10)
                End If
            End If
        End While

        patcherUrl = New Uri("file:///" + My.Computer.FileSystem.CurrentDirectory + "\index.html")
        WebControl1.Source = patcherUrl

        Dim Service As New WebService(serverURL)
        Dim res As WebServiceResult = Service.getData("MasterIndex")

        If res.Elements.Item(1).Name = "MasterIndex" Then
            For Each c As Integer In res.Elements.Item(1).Childs
                If (res.Elements.Item(c).Name = "Config") Then
                    Me.ConfigUrl = res.Elements.Item(c).Text
                End If
                If (res.Elements.Item(c).Name = "Status") Then
                    Me.StatusUrl = res.Elements.Item(c).Text
                End If
            Next
        End If

        Dim ConfigService As New WebService(Me.ConfigUrl)
        Env = Service.getData("EnvironmentInfo")

        Dim GameInfo As Integer = Env.getElement("GameInfo")
        If (GameInfo > 0) Then
            Dim LauncherURL2 As Integer = Env.getElement("LauncherUrl2", GameInfo)
            If (LauncherURL2 > 0) Then
                Me.launcherURL = Env.Elements.Item(LauncherURL2).Text
            End If
        End If

        Dim serversString As String = ""

        Dim Servers As Integer = Env.getElement("Servers")
        For Each server As Integer In Env.Elements.Item(Servers).Childs
            If Env.Elements.Item(server).Name = "Server" Then
                Dim Info As New ServerInfo()
                For Each el As Integer In Env.Elements.Item(server).Childs
                    With Env.Elements.Item(el)
                        Select Case .Name
                            Case "Name"
                                Info.Name = .Text
                            Case "AuthenticationIP"
                                Info.Address = .Text
                            Case "Suggested"
                                If (.Text = "true") Then
                                    Info.Suggested = True
                                Else
                                    Info.Suggested = False
                                End If
                        End Select
                    End With
                Next
                ServerList.Add(Info)
                If (Info.Suggested And Suggested = 0) Then
                    Suggested = ServerList.Count
                End If
            End If
        Next

        If (Suggested > 0) Then
            Selected = Suggested - 1
        End If
    End Sub

    Private Function play(arguments() As JSValue) As JSValue
        'Dim FRAME_STYLE As JSObject = FRAME.Property("style")
        'FRAME_STYLE.Property("display") = "none"

        Dim boot As New LDF
        boot.WriteString("SERVERNAME", ServerList(Selected).Name)
        boot.WriteString("PATCHSERVERIP", "services.lego.com")
        boot.WriteString("AUTHSERVERIP", ServerList(Selected).Address)
        boot.WriteInteger("PATCHSERVERPORT", 80)
        boot.WriteInteger("LOGGING", 100)
        boot.WriteUnsignedInt("DATACENTERID", "1")
        boot.WriteInteger("CPCODE", 89164)
        boot.WriteBoolean("AKAMAIDLM", False)
        boot.WriteString("PATCHSERVERDIR", "UniversePatcher/lu/luclient")
        boot.WriteBoolean("UGCUSE3DSERVICES", True)
        boot.WriteString("UGCSERVERIP", "services.lego.com")
        boot.WriteString("UGCSERVERDIR", "UniversePatcher/lu/luclient")
        boot.WriteString("PASSURL", "http://services.lego.com/UniverseLauncher/index.php?page=sendPassword&username=")
        boot.WriteString("SIGNINURL", "http://services.lego.com/UniverseLauncher/index.php")
        boot.WriteString("SIGNUPURL", "http://services.lego.com/UniverseLauncher/index.php?page=register")
        boot.WriteString("REGISTERURL", "http://services.lego.com/UniverseLauncher/Registration.php?username=")
        boot.WriteString("CRASHLOGURL", "http://services.lego.com/UniverseLauncher/CrashLog.php")
        boot.WriteString("LOCALE", "en_US")
        boot.WriteString("MANIFESTFILE", "trunk.txt")
        boot.WriteBoolean("TRACK_DSK_USAGE", True)
        boot.WriteUnsignedInt("HD_SPACE_FREE", 1244987)
        boot.WriteUnsignedInt("HD_SPACE_USED", 11422)
        boot.WriteBoolean("USE_CATALOG", True)

        Dim bootFile As String = boot.getResult()

        My.Computer.FileSystem.WriteAllText(clientURL & "\boot.cfg", bootFile, False)

        'MsgBox(bootFile)

        Dim clientProcess As New Process
        Dim f As String = clientURL & "\legouniverse.exe"
        'MsgBox(f)
        clientProcess.StartInfo.FileName = f
        clientProcess.StartInfo.WorkingDirectory = clientURL
        clientProcess.Start()
        Return JSValue.Null
    End Function

    Private Function closeButton(arguments() As JSValue) As JSValue
        WebControl1.Dispose()
        WebCore.Shutdown()
        Me.Close()
        End
    End Function

    Private Function minimizeButton(arguments() As JSValue) As JSValue
        Me.WindowState = FormWindowState.Minimized
        Return JSValue.Null
    End Function

    Private Sub WebControl1_DocumentReady(sender As Object, e As DocumentReadyEventArgs) Handles WebControl1.DocumentReady
        If e.Url.AbsoluteUri.Equals(Me.patcherUrl.AbsoluteUri) Then
            If (e.ReadyState = DocumentReadyState.Ready) Then
                'MsgBox("Document Ready")
            End If
            If (e.ReadyState = DocumentReadyState.Loaded) Then
                'MsgBox("Document Loaded")
            End If
        End If
    End Sub

    Private Sub WebControl1_NativeViewInitialized(sender As Object, e As WebViewEventArgs) Handles WebControl1.NativeViewInitialized
        Dim js As Awesomium.Core.JSObject = WebControl1.CreateGlobalJavascriptObject("patcher")
        js.BindAsync("ready", New JSFunctionAsyncHandler(AddressOf ready))
        js.BindAsync("selectUniverse", New JSFunctionAsyncHandler(AddressOf selectUniverse))
    End Sub

    Private Function ready(arguments() As JSValue) As JSValue
        FRAME = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""frame"")")
        Dim CLOSE As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""close"")")
        Dim MINIMIZE As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""minimize"")")
        Dim START As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""start"")")
        Dim SETTINGS As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""settings"")")
        Dim HELP As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""help"")")
        Dim UNIVERSE As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""universe"")")
        Dim USELECTS As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""uselect-s"")")
        Dim UCLOSE As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""uclose"")")
        FRAME.Property("src") = Me.launcherURL
        CLOSE.BindAsync("onclick", AddressOf closeButton)
        MINIMIZE.BindAsync("onclick", AddressOf minimizeButton)
        START.BindAsync("onclick", AddressOf play)
        SETTINGS.BindAsync("onclick", AddressOf openSettings)
        HELP.BindAsync("onclick", AddressOf openHelp)
        UNIVERSE.BindAsync("onclick", AddressOf openUniverseSelection)
        UCLOSE.BindAsync("onclick", AddressOf closeUniverseSelection)

        If (ServerList.Count > 0) Then
            UNIVERSE.Property("name") = ServerList.Item(Selected).Name
            For k = 0 To ServerList.Count - 1
                Dim newElement As JSObject = WebControl1.ExecuteJavascriptWithResult("document.createElement('a')")
                newElement.Property("id") = "universe-" & k.ToString
                newElement.Property("innerHTML") = ServerList.Item(k).Name
                If (k = Selected) Then
                    newElement.Property("className") = "selected"
                End If
                newElement.BindAsync("onclick", New JSFunctionAsyncHandler(AddressOf selectUniverse))
                USELECTS.Invoke("appendChild", {newElement})
            Next
        End If
        Return JSValue.Null
    End Function

    Private Function openSettings(arguments() As JSValue) As JSValue
        WebControl1.Reload(True)
        Return JSValue.Null
    End Function

    Private Function openUniverseSelection(arguments() As JSValue) As JSValue
        WebControl1.ExecuteJavascript("document.getElementById('universe-selection').style.display = 'block'")
        Return JSValue.Null
    End Function

    Private Function closeUniverseSelection(arguments() As JSValue) As JSValue
        WebControl1.ExecuteJavascript("document.getElementById('universe-selection').style.display = 'none'")
        Return JSValue.Null
    End Function

    Private Function openHelp(arguments() As JSValue) As JSValue
        Process.Start("https://github.com/LUNIServer/")
        Return JSValue.Null
    End Function

    Private Function selectUniverse(arguments() As JSValue) As JSValue
        If (arguments.Length > 0) Then
            If (arguments(0).IsObject) Then
                Dim ev As JSObject = arguments(0)
                Dim t As JSObject = ev.Property("target")
                ClearList()
                t.Property("className") = "selected"
                Dim num As String = t.Property("id").ToString().Substring(9)
                Dim id As Integer = Integer.Parse(num)
                Selected = id
                WebControl1.ExecuteJavascript("document.getElementById(""universe"").name = '" & ServerList(id).Name & "'")
            End If
        End If
        Return JSValue.Null
    End Function

    Private Sub ClearList()
        For k = 0 To ServerList.Count - 1
            WebControl1.ExecuteJavascript("document.getElementById('universe-" & k & "').className = ''")
        Next
    End Sub
End Class
