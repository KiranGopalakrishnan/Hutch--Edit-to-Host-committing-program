Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Windows.Forms

Public Class sync
    Dim filePath As String
    Dim lastCheck As String
    Dim provider As CultureInfo = CultureInfo.InvariantCulture
    Public Sub dirAction(ByVal dirInfo As DirectoryInfo)
        ' do something here '
        ' Dim files As FileInfo()
        'files = dirInfo.GetFiles()
        'Dim lastCheckDate As DateTime = DateTime.Parse(lastCheck)
        'Dim fri As FileInfo
        'For Each fri In files
        'Console.WriteLine(fri.LastWriteTime)
        'If fri.LastWriteTime > lastCheckDate Then
        'listView.Items.Add(fri.Directory.Name + "/" + fri.Name)
        'End If
        'Next fri
    End Sub
    Public Sub fileAction(ByVal fileInfo As FileInfo)
        ' do something here '
        Dim lastCheckDate As DateTime = DateTime.Parse(lastCheck)
        If fileInfo.LastWriteTime > lastCheckDate Then
            ' MsgBox(fileInfo.FullName)'
            Dim fileP As String = Replace(fileInfo.FullName, filePath, "")
            listView.Items.Add(fileP)
        End If
        'Console.WriteLine(fileInfo.FullName)

    End Sub

    Public Sub uploadFTP(file As String, fileName As String)
        Dim user, pass, host As String
        Dim f As File
        Dim hostData() As String = f.ReadAllText("./hostData.txt").Split("|"c)
        user = hostData(0)
        pass = hostData(1)
        host = hostData(2)
        MsgBox(user)
        If host(host.Length - 1) = "/" Or host(host.Length - 1) = "\" Then
            host = host.Remove(host.Length - 1)
        End If
        Dim ftpHost As String = host

        fileName = Replace(fileName, "\", "/")
        Dim miUri As String = ftpHost + fileName
        'MsgBox(miUri)

        Dim request As FtpWebRequest = FtpWebRequest.Create(ftpHost)
        Dim creds As NetworkCredential = New NetworkCredential(user, pass)
        request.Credentials = creds

        Dim resp As FtpWebResponse = Nothing
        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails
        request.KeepAlive = True
        Using resp
            resp = request.GetResponse()
            Dim sr As StreamReader = New StreamReader(resp.GetResponseStream(), System.Text.Encoding.ASCII)
            Dim s As String = sr.ReadToEnd()
            Dim ftpNPath As String = Replace(miUri, New FileInfo(file).Name, "")
            MsgBox(Replace(miUri, New FileInfo(file).Name, ""))
            Dim tempFold() As String = Replace(ftpNPath, ftpHost + "/", "").Split("/")
            Dim foldName As String = ""
            Dim bool As Boolean = False
            If Not s.Contains(tempFold(0)) Then
                For j As Integer = 0 To (tempFold.Count - 2)

                    Console.Write("TempFold" + j.ToString + ":" + Replace(ftpNPath, ftpHost + "/", ""))
                    request = FtpWebRequest.Create(ftpHost + "/" + foldName + tempFold(j))
                    MsgBox(ftpHost + "/" + tempFold(j))
                    request.Credentials = creds
                    request.Method = WebRequestMethods.Ftp.MakeDirectory
                    resp = request.GetResponse()
                    Console.WriteLine(resp.StatusCode & "Created")
                    If foldName Is "" Then
                        foldName = tempFold(j) + "/"
                    Else
                        foldName = foldName + tempFold(j) + "/"
                    End If
                    MsgBox(foldName)
                Next j
            Else
                Console.WriteLine("Directory already exists")
            End If


        End Using


        Dim miRequest As Net.FtpWebRequest = Net.WebRequest.Create(miUri)
        miRequest.Credentials = New Net.NetworkCredential(user, pass)
        miRequest.Method = Net.WebRequestMethods.Ftp.UploadFile
        Try
            Dim bFile() As Byte = System.IO.File.ReadAllBytes(file)
            Dim miStream As System.IO.Stream = miRequest.GetRequestStream()
            miStream.Write(bFile, 0, bFile.Length)
            miStream.Close()
            miStream.Dispose()
            label1.Content = "Changes Has Been Committed To Host"
        Catch ex As Exception
            Throw New Exception(ex.Message & "Unable to transfer files")
        End Try
    End Sub
    Public Shared Sub ForEachFileAndFolder(ByVal sourceFolder As String,
                                       ByVal directoryCallBack As Action(Of DirectoryInfo),
                                       ByVal fileCallBack As Action(Of FileInfo))

        If Directory.Exists(sourceFolder) Then
            Try
                For Each foldername As String In Directory.GetDirectories(sourceFolder)
                    If directoryCallBack IsNot Nothing Then
                        directoryCallBack.Invoke(New DirectoryInfo(foldername))
                    End If

                    ForEachFileAndFolder(foldername, directoryCallBack, fileCallBack)
                Next
            Catch ex As UnauthorizedAccessException
                Trace.TraceWarning(ex.Message)
            End Try

            If fileCallBack IsNot Nothing Then

                For Each filename As String In Directory.GetFiles(sourceFolder)
                    fileCallBack.Invoke(New FileInfo(filename))
                Next
            End If
        End If
    End Sub

    Private Sub button1_Click(sender As Object, e As RoutedEventArgs) Handles button1.Click
        For i As Integer = 0 To (listView.Items.Count - 1)
            Dim fI As New FileInfo(listView.Items.GetItemAt(i))
            Dim filename As String = fI.Name
            uploadFTP(filePath + listView.Items.GetItemAt(i), listView.Items.GetItemAt(i))
            progressBar1.Value = (i * (100 / listView.Items.Count) / listView.Items.Count) * 100
        Next i
        Dim nowdate As String = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
        File.WriteAllText("./changesData.txt", String.Join("|", New String() {nowdate}))
        lastCheck = nowdate
        listView.Items.Clear()
        MsgBox("Files Have Been Synced")
        progressBar1.Value = 0
    End Sub

    Private Sub MenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim a As New Window1
        a.ShowDialog()

    End Sub

    Private Sub sync_GotFocus(sender As Object, e As RoutedEventArgs) Handles Me.GotFocus

    End Sub

    Private Sub sync_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Dim t As New Timer With {.Interval = 1000}
        t.Tag = DateTime.Now
        AddHandler t.Tick, AddressOf MyTickHandler
        t.Start()
    End Sub
    Sub MyTickHandler(ByVal sender As Object, ByVal e As EventArgs)
        Dim t As Timer = DirectCast(sender, Timer)
        Dim values() As String = File.ReadAllText("./projectData.txt").Split("|"c)
        Dim checkValues() As String = File.ReadAllText("./changesData.txt").Split("|"c)
        filePath = values(0)
        lastCheck = checkValues(0)
        listView.Items.Clear()
        If lastCheck Is "" Then
            Dim nowdate As String = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
            File.WriteAllText("./changesData.txt", String.Join("|", New String() {nowdate}))
            lastCheck = nowdate
        End If
        ForEachFileAndFolder(filePath, AddressOf dirAction, AddressOf fileAction)
        If listView.Items.Count = 0 Then
            label1.Content = "No Changes Found"
        Else
            label1.Content = " "
        End If
    End Sub
End Class
