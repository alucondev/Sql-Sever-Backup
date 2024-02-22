﻿Imports System.Data.Sql
Imports System.ServiceProcess
Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
    Private Sub PopulateSqlServerInstances()
        ' Create an instance of SqlDataSourceEnumerator
        Dim instance As SqlDataSourceEnumerator = SqlDataSourceEnumerator.Instance

        Try
            ' Retrieve the list of available SQL Server instances
            Dim table As System.Data.DataTable = instance.GetDataSources()
            DataGridView1.DataSource = table
            '' Iterate through the table and add instances to the ComboBox
            'For Each row As System.Data.DataRow In table.Rows
            '    Dim serverName As String = row("ServerName").ToString()
            '    Dim instanceName As String = row("InstanceName").ToString()

            '    ' Check if the instance has a specific name
            '    If String.IsNullOrEmpty(instanceName) Then
            '        ' Add the server name to the ComboBox
            '        ComboBox1.Items.Add(serverName)
            '    Else
            '        ' Add the server name and instance name to the ComboBox
            '        ComboBox1.Items.Add(serverName & "\" & instanceName)
            '    End If
            'Next

            ' Select the first item in the ComboBox
            If Source.Items.Count > 0 Then
                Source.SelectedIndex = 0
            End If
        Catch ex As Exception
            ' Handle exception (e.g., if unable to retrieve instances)
            MessageBox.Show("Error retrieving SQL Server instances: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim conn As String = $"Data Source={Source.Text};User Id={User_ID.Text};Password={Password.Text};"
        Dim sqlHelper As New SqlServerHelper(conn)
        If sqlHelper.TestConnection(conn) Then
            ' Connection test successful
            Console.WriteLine("Connection test successful.")
            Catalog.DataSource = sqlHelper.GetAllDatabaseNames()
        Else
            ' Connection test failed
            Console.WriteLine("Connection test failed.")
        End If



    End Sub

    Private Function BrowseFile(ByVal title As String, ByVal filter As String, ByVal initialDirectory As String) As String
        Dim openFileDialog As New OpenFileDialog()
        openFileDialog.Title = title
        openFileDialog.Filter = filter
        openFileDialog.InitialDirectory = initialDirectory

        If openFileDialog.ShowDialog = DialogResult.OK Then
            Return openFileDialog.FileName
        End If

        Return String.Empty
    End Function
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim encryptor As New FileEncryptor()
        Dim selectedFilePath As String = Nothing
        ' Check if passwords match
        Try
            selectedFilePath = BrowseFile("Choose a file to decrypt", "Encrypted Files (*.encrypt)|*.encrypt", "C:\")
            If Not String.IsNullOrEmpty(selectedFilePath) Then
                selectedFilePath = selectedFilePath
                encryptor.DecryptFile(tselectedFilePath, txtDestinationDecrypt.Text, txtPassDecrypt.Text)
                MessageBox.Show("Decryption Complete", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ' Update UI or perform other actions as needed
            End If
        Catch ex As Exception
            MessageBox.Show("Decryption failed: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        '' Encrypt file
        'Try
        '       selectedFilePath = BrowseFile("Choose a file to encrypt", "All Files (*.*)|*.*", "C:\")
        '        If Not String.IsNullOrEmpty(selectedFilePath) Then
        '            Dim txtFileToEncrypt = selectedFilePath
        '        encryptor.EncryptFile(txtFileToEncrypt, txtFileToEncrypt & ".111", "123")
        '        MessageBox.Show("Encryption Complete", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
        '            ' Update UI or perform other actions as needed
        '        End If
        '    Catch ex As Exception
        '        MessageBox.Show("Encryption failed: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        '    End Try

    End Sub
End Class
