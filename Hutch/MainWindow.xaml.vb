Imports System.IO
Imports System.Windows.Forms.FolderBrowserDialog

Class MainWindow
    Dim Dialog As Forms.FolderBrowserDialog
    Dim synWindow As sync
    Dim path As String
    Private Sub Select_Project_Click(sender As Object, e As RoutedEventArgs) Handles Select_Project.Click
        Dialog = New Forms.FolderBrowserDialog()
        Dialog.ShowDialog()
        path = Dialog.SelectedPath
        File.WriteAllText("./projectData.txt", String.Join("|", New String() {path}))
        synWindow = New sync()
        synWindow.Show()

    End Sub

    Private Sub Select_Project_Loaded(sender As Object, e As RoutedEventArgs) Handles Select_Project.Loaded
        If System.IO.File.Exists("./projectData.txt") Then
            Dim values() As String = File.ReadAllText("./projectData.txt").Split("|"c)
            If values(0) IsNot "" Then
                synWindow = New sync()
                synWindow.Show()
                Me.Close()
            End If
        End If

    End Sub
End Class
