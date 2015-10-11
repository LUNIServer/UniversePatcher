Imports Awesomium.Core
Imports System.IO

Public Class Form1
    Dim LauncherFrameObject As JSObject
    Dim LauncherUniverseButton As JSObject

    Dim Env As WebServiceResult = Nothing
    'AccountData
    Dim SendPasswordUrl As String
    Dim SignInUrl As String
    Dim SignUpUrl As String
    'GameData
    Dim ClientUrl As String
    Dim CrashLogUrl As String
    Dim launcherURL As String

    'Servers
    Dim ServerList As New List(Of ServerInfo)
    Dim Suggested As Integer = 0
    Dim Selected As Integer = 0
    '--------

    Dim ConfigUrl As String
    Dim StatusUrl As String

    Dim clientPath As String
    Dim serverURL As String
    Dim ininame As String

    Dim patcherUrl As Uri

    Dim Language As String = "default"

    'Awesomium
    Dim FRAME As JSObject

    Dim L_US As JSObject
    Dim L_UK As JSObject
    Dim L_DE As JSObject
    Dim NOL As JSObject
    Dim CP As JSObject

    Public Structure ServerInfo
        Public AuthenticationIP As String
        Public CdnInfo As CdnInfo
        Public DataCenterId As Integer
        Public Language As String
        Public LogLevel As Integer
        Public Name As String
        Public Suggested As Boolean
        Public UgcCdnInfo As CdnInfo
        Public Use3DServices As Boolean
    End Structure

    Public Structure CdnInfo
        Public CpCode As Integer
        Public PatcherDir As String
        Public PatcherUrl As String
        Public Secure As Boolean
        Public UseDlm As Boolean
    End Structure

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ininame = My.Computer.FileSystem.CurrentDirectory & "\lunipatcher.ini"

        serverURL = "http://luniserver.com/UniverseConfig/UniverseConfig.svc"
        clientPath = "..\client"

        If (My.Computer.FileSystem.FileExists(ininame)) Then
            Dim INI As StreamReader = My.Computer.FileSystem.OpenTextFileReader(ininame)
            Dim line As String = ""

            While Not INI.EndOfStream
                line = INI.ReadLine()
                If (line.Length > 10) Then
                    If (line.StartsWith("ConfigUrl=")) Then
                        serverURL = line.Substring(10)
                    End If
                    If (line.StartsWith("ClientUrl=")) Then
                        clientPath = line.Substring(10)
                    End If
                End If
            End While

            INI.Close()
        Else
            WriteConfigFile()
        End If

        OpenFileDialog1.InitialDirectory = clientPath

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

        Dim AccountInfo As Integer = Env.getElement("AccountInfo")
        If (AccountInfo > 0) Then
            Dim SendPasswordUrlNode As Integer = Env.getElement("SendPasswordUrl", AccountInfo)
            If (SendPasswordUrlNode > 0) Then
                Me.SendPasswordUrl = Env.Elements.Item(SendPasswordUrlNode).Text
            End If
            Dim SignInUrlNode As Integer = Env.getElement("SignInUrl", AccountInfo)
            If (SignInUrlNode > 0) Then
                Me.SignInUrl = Env.Elements.Item(SignInUrlNode).Text
            End If
            Dim SignUpUrlNode As Integer = Env.getElement("SignUpUrl", AccountInfo)
            If (SignUpUrlNode > 0) Then
                Me.SignUpUrl = Env.Elements.Item(SignUpUrlNode).Text
            End If
        End If

        Dim GameInfo As Integer = Env.getElement("GameInfo")
        If (GameInfo > 0) Then
            Dim LauncherURL2 As Integer = Env.getElement("LauncherUrl2", GameInfo)
            If (LauncherURL2 > 0) Then
                Me.launcherURL = Env.Elements.Item(LauncherURL2).Text
            End If
            Dim ClientUrlNode As Integer = Env.getElement("ClientUrl", GameInfo)
            If (ClientUrlNode > 0) Then
                Me.ClientUrl = Env.Elements.Item(ClientUrlNode).Text
            End If
            Dim CrashLogUrlNode As Integer = Env.getElement("CrashLogUrl", GameInfo)
            If (CrashLogUrlNode > 0) Then
                Me.CrashLogUrl = Env.Elements.Item(CrashLogUrlNode).Text
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
                                Info.AuthenticationIP = .Text
                            Case "Suggested"
                                If (.Text = "true") Then
                                    Info.Suggested = True
                                Else
                                    Info.Suggested = False
                                End If
                            Case "CdnInfo"
                                For Each cdni As Integer In .Childs
                                    Dim cde As Element = Env.Elements.Item(cdni)
                                    Select Case cde.Name
                                        Case "CpCode"
                                            Info.CdnInfo.CpCode = cde.Text
                                        Case "PatcherDir"
                                            Info.CdnInfo.PatcherDir = cde.Text
                                        Case "PatcherUrl"
                                            Info.CdnInfo.PatcherUrl = cde.Text
                                        Case "Secure"
                                            Info.CdnInfo.Secure = Boolean.Parse(cde.Text)
                                        Case "UseDlm"
                                            Info.CdnInfo.UseDlm = Boolean.Parse(cde.Text)
                                    End Select
                                Next
                            Case "DataCenterId"
                                Info.DataCenterId = Integer.Parse(.Text)
                            Case "LogLevel"
                                Info.LogLevel = Integer.Parse(.Text)
                            Case "Language"
                                Info.Language = .Text
                            Case "Use3DServices"
                                Info.Use3DServices = Boolean.Parse(.Text)
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

    Private Sub WriteConfigFile()
        Dim writer As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(ininame, False)
        writer.WriteLine("[General]")
        writer.WriteLine("ConfigUrl=" & serverURL)
        writer.WriteLine("ClientUrl=" & clientPath)
        writer.Flush()
        writer.Close()
    End Sub

    Private Function play(arguments() As JSValue) As JSValue
        'Dim FRAME_STYLE As JSObject = FRAME.Property("style")
        'FRAME_STYLE.Property("display") = "none"

        Dim boot As New LDF
        boot.WriteString("SERVERNAME", ServerList(Selected).Name)
        boot.WriteString("PATCHSERVERIP", ServerList(Selected).CdnInfo.PatcherUrl)
        boot.WriteString("AUTHSERVERIP", ServerList(Selected).AuthenticationIP)
        boot.WriteInteger("PATCHSERVERPORT", 80) 'Currently default, need parsing
        boot.WriteInteger("LOGGING", ServerList(Selected).LogLevel)
        boot.WriteUnsignedInt("DATACENTERID", ServerList(Selected).DataCenterId)
        boot.WriteInteger("CPCODE", ServerList(Selected).CdnInfo.CpCode)
        boot.WriteBoolean("AKAMAIDLM", ServerList(Selected).CdnInfo.Secure)
        boot.WriteString("PATCHSERVERDIR", ServerList(Selected).CdnInfo.PatcherDir)
        boot.WriteBoolean("UGCUSE3DSERVICES", ServerList(Selected).Use3DServices)
        boot.WriteString("UGCSERVERIP", ServerList(Selected).UgcCdnInfo.PatcherUrl)
        boot.WriteString("UGCSERVERDIR", ServerList(Selected).UgcCdnInfo.PatcherDir)
        boot.WriteString("PASSURL", Me.SendPasswordUrl)
        boot.WriteString("SIGNINURL", Me.SignInUrl)
        boot.WriteString("SIGNUPURL", Me.SignUpUrl)
        boot.WriteString("REGISTERURL", Me.ClientUrl)
        boot.WriteString("CRASHLOGURL", Me.CrashLogUrl)

        If (Language = "default") Then
            boot.WriteString("LOCALE", ServerList(Selected).Language)
        Else
            boot.WriteString("LOCALE", Language)
        End If


        boot.WriteString("MANIFESTFILE", "trunk.txt")
        boot.WriteBoolean("TRACK_DSK_USAGE", True)
        boot.WriteUnsignedInt("HD_SPACE_FREE", 1244987)
        boot.WriteUnsignedInt("HD_SPACE_USED", 11422)
        boot.WriteBoolean("USE_CATALOG", True)

        Dim bootFile As String = boot.getResult()

        My.Computer.FileSystem.WriteAllText(clientPath & "\boot.cfg", bootFile, False)

        'MsgBox(bootFile)

        Dim clientProcess As New Process
        Dim f As String = clientPath & "\legouniverse.exe"
        'MsgBox(f)
        clientProcess.StartInfo.FileName = f
        clientProcess.StartInfo.WorkingDirectory = clientPath
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
        Dim SCLOSE As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""sclose"")")
        Dim BROWSE As JSObject = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""chooseClient"")")
        CP = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""CP"")")

        L_US = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""lus"")")
        L_UK = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""luk"")")
        L_DE = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""lde"")")
        NOL = WebControl1.ExecuteJavascriptWithResult("document.getElementById(""nl"")")

        FRAME.Property("src") = Me.launcherURL
        CLOSE.BindAsync("onclick", AddressOf closeButton)
        MINIMIZE.BindAsync("onclick", AddressOf minimizeButton)
        START.BindAsync("onclick", AddressOf play)
        SETTINGS.BindAsync("onclick", AddressOf openSettings)
        HELP.BindAsync("onclick", AddressOf openHelp)
        UNIVERSE.BindAsync("onclick", AddressOf openUniverseSelection)
        UCLOSE.BindAsync("onclick", AddressOf closeUniverseSelection)
        SCLOSE.BindAsync("onclick", AddressOf closeSettings)
        BROWSE.BindAsync("onclick", AddressOf chooseClient)

        L_US.BindAsync("onclick", AddressOf setAmerican)
        L_UK.BindAsync("onclick", AddressOf setBritish)
        L_DE.BindAsync("onclick", AddressOf setGerman)
        NOL.BindAsync("onclick", AddressOf setServerDefault)

        CP.Property("value") = Me.clientPath

        If (ServerList.Count > 0) Then
            UNIVERSE.Property("name") = ServerList.Item(Selected).Name
            For k = 0 To ServerList.Count - 1
                Dim newElement As JSObject = WebControl1.ExecuteJavascriptWithResult("document.createElement('a')")
                newElement.Property("id") = "universe-" & k.ToString
                newElement.Property("innerHTML") = ServerList.Item(k).Name
                Dim classname As String = ""
                If (ServerList.Item(k).Language.Equals("en_US")) Then
                    classname = "us"
                End If
                If (ServerList.Item(k).Language.Equals("en_GB")) Then
                    classname = "uk"
                End If
                If (ServerList.Item(k).Language.Equals("de_DE")) Then
                    classname = "de"
                End If
                If (k = Selected) Then
                    If (Not classname.Equals("")) Then
                        classname = classname + " "
                    End If
                    classname = classname + "selected"
                End If
                newElement.Property("className") = classname
                newElement.BindAsync("onclick", New JSFunctionAsyncHandler(AddressOf selectUniverse))
                USELECTS.Invoke("appendChild", {newElement})
            Next
        End If

        UpdateLanguage()

        Return JSValue.Null
    End Function

    Private Function chooseClient(arguments() As JSValue) As JSValue
        OpenFileDialog1.ShowDialog()
        If (OpenFileDialog1.CheckFileExists) Then
            Dim f As FileInfo = New FileInfo(OpenFileDialog1.FileName)
            Dim d As DirectoryInfo = f.Directory
            If (d.Name.Equals("client")) Then
                Me.clientPath = d.FullName
                CP.Property("value") = d.FullName
                OpenFileDialog1.InitialDirectory = d.FullName
                WriteConfigFile()
            End If
        End If
        Return JSValue.Null
    End Function

    Private Function setGerman(arguments() As JSValue) As JSValue
        Me.Language = "de_DE"
        L_US.Property("className") = "us"
        L_UK.Property("className") = "uk"
        L_DE.Property("className") = "de selected"
        NOL.Property("className") = "nolang"
        UpdateLanguage()
        Return JSValue.Null
    End Function

    Private Function setAmerican(arguments() As JSValue) As JSValue
        Me.Language = "en_US"
        L_US.Property("className") = "us selected"
        L_UK.Property("className") = "uk"
        L_DE.Property("className") = "de"
        NOL.Property("className") = "nolang"
        UpdateLanguage()
        Return JSValue.Null
    End Function

    Private Function setBritish(arguments() As JSValue) As JSValue
        Me.Language = "en_GB"
        L_US.Property("className") = "us"
        L_UK.Property("className") = "uk selected"
        L_DE.Property("className") = "de"
        NOL.Property("className") = "nolang"
        UpdateLanguage()
        Return JSValue.Null
    End Function

    Private Function setServerDefault(arguments() As JSValue) As JSValue
        Me.Language = "default"
        L_US.Property("className") = "us"
        L_UK.Property("className") = "uk"
        L_DE.Property("className") = "de"
        NOL.Property("className") = "nolang selected"
        UpdateLanguage()
        Return JSValue.Null
    End Function

    Private Function openSettings(arguments() As JSValue) As JSValue
        WebControl1.ExecuteJavascript("document.getElementById('settings-panel').style.display = 'block'")
        Return JSValue.Null
    End Function

    Private Function closeSettings(arguments() As JSValue) As JSValue
        WebControl1.ExecuteJavascript("document.getElementById('settings-panel').style.display = 'none'")
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
        Process.Start("https://github.com/LUNIServer/UniversePatcher/wiki")
        'WebControl1.Reload(True)
        Return JSValue.Null
    End Function

    Private Function selectUniverse(arguments() As JSValue) As JSValue
        If (arguments.Length > 0) Then
            If (arguments(0).IsObject) Then
                Dim ev As JSObject = arguments(0)
                Dim t As JSObject = ev.Property("target")
                Dim num As String = t.Property("id").ToString().Substring(9)
                Dim id As Integer = Integer.Parse(num)
                Selected = id
                RedrawList()
                WebControl1.ExecuteJavascript("document.getElementById(""universe"").name = '" & ServerList(id).Name & "'")
                UpdateLanguage()
            End If
        End If
        Return JSValue.Null
    End Function

    Private Sub RedrawList()
        For k = 0 To ServerList.Count - 1
            Dim classname As String = ""
            If (ServerList.Item(k).Language.Equals("en_US")) Then
                classname = "us"
            End If
            If (ServerList.Item(k).Language.Equals("en_GB")) Then
                classname = "uk"
            End If
            If (ServerList.Item(k).Language.Equals("de_DE")) Then
                classname = "de"
            End If
            If (k = Selected) Then
                If (Not classname.Equals("")) Then
                    classname = classname + " "
                End If
                classname = classname + "selected"
            End If
            WebControl1.ExecuteJavascript("document.getElementById('universe-" & k & "').className = '" & classname & "'")
        Next
    End Sub

    Private Sub UpdateLanguage()
        Dim classname As String = "button "
        If (Language.Equals("default")) Then
            If (ServerList.Item(Selected).Language.Equals("en_US")) Then
                classname = classname & "us"
            End If
            If (ServerList.Item(Selected).Language.Equals("en_GB")) Then
                classname = classname & "uk"
            End If
            If (ServerList.Item(Selected).Language.Equals("de_DE")) Then
                classname = classname & "de"
            End If
        Else
            If (Language.Equals("en_US")) Then
                classname = classname & "us"
            End If
            If (Language.Equals("en_GB")) Then
                classname = classname & "uk"
            End If
            If (Language.Equals("de_DE")) Then
                classname = classname & "de"
            End If
        End If
        WebControl1.ExecuteJavascript("document.getElementById(""universe"").className = '" & classname & "'")
    End Sub
End Class
