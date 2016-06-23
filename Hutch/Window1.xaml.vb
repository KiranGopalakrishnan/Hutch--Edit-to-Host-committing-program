Imports System.IO

Public Class Window1
    Private Sub button_Click(sender As Object, e As RoutedEventArgs) Handles button.Click
        Dim user, pass, host As String
        user = userBox.Text
        host = hostBox.Text
        pass = passwordBox.Password
        File.WriteAllText("./hostData.txt", String.Join("|", New String() {user, pass, host}))
        Me.Close()
    End Sub
End Class
