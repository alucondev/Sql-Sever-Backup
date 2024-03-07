Imports System.IO
Imports System.Security.Cryptography

Public Class FileEncryptorDecryptor

    '*************************
    '** Global Variables
    '*************************
    Private strFileToEncrypt As String
    Private strFileToDecrypt As String
    Private strOutputEncrypt As String
    Private strOutputDecrypt As String
    Private fsInput As FileStream
    Private fsOutput As FileStream

    '*************************
    '** Create A Key
    '*************************
    Private Function CreateKey(ByVal strPassword As String) As Byte()
        'Convert strPassword to an array and store in chrData.
        Dim chrData() As Char = strPassword.ToCharArray
        'Use intLength to get strPassword size.
        Dim intLength As Integer = chrData.GetUpperBound(0)
        'Declare bytDataToHash and make it the same size as chrData.
        Dim bytDataToHash(intLength) As Byte

        'Use For Next to convert and store chrData into bytDataToHash.
        For i As Integer = 0 To chrData.GetUpperBound(0)
            bytDataToHash(i) = CByte(Asc(chrData(i)))
        Next

        'Declare what hash to use.
        Dim SHA512 As New System.Security.Cryptography.SHA512Managed
        'Declare bytResult, Hash bytDataToHash and store it in bytResult.
        Dim bytResult As Byte() = SHA512.ComputeHash(bytDataToHash)
        'Declare bytKey(31).  It will hold 256 bits.
        Dim bytKey(31) As Byte

        'Use For Next to put a specific size (256 bits) of 
        'bytResult into bytKey. The 0 To 31 will put the first 256 bits
        'of 512 bits into bytKey.
        For i As Integer = 0 To 31
            bytKey(i) = bytResult(i)
        Next

        Return bytKey 'Return the key.
    End Function

    '*************************
    '** Create An IV
    '*************************
    Private Function CreateIV(ByVal strPassword As String) As Byte()
        'Convert strPassword to an array and store in chrData.
        Dim chrData() As Char = strPassword.ToCharArray
        'Use intLength to get strPassword size.
        Dim intLength As Integer = chrData.GetUpperBound(0)
        'Declare bytDataToHash and make it the same size as chrData.
        Dim bytDataToHash(intLength) As Byte

        'Use For Next to convert and store chrData into bytDataToHash.
        For i As Integer = 0 To chrData.GetUpperBound(0)
            bytDataToHash(i) = CByte(Asc(chrData(i)))
        Next

        'Declare what hash to use.
        Dim SHA512 As New System.Security.Cryptography.SHA512Managed
        'Declare bytResult, Hash bytDataToHash and store it in bytResult.
        Dim bytResult As Byte() = SHA512.ComputeHash(bytDataToHash)
        'Declare bytIV(15).  It will hold 128 bits.
        Dim bytIV(15) As Byte

        'Use For Next to put a specific size (128 bits) of 
        'bytResult into bytIV. The 0 To 30 for bytKey used the first 256 bits.
        'of the hashed password. The 32 To 47 will put the next 128 bits into bytIV.
        For i As Integer = 32 To 47
            bytIV(i - 32) = bytResult(i)
        Next

        Return bytIV 'return the IV
    End Function

    '****************************
    '** Encrypt/Decrypt File
    '****************************
    Private Enum CryptoAction
        ActionEncrypt = 1
        ActionDecrypt = 2
    End Enum
    Private Sub EncryptOrDecryptFile(ByVal strInputFile As String, ByVal strOutputFile As String, ByVal bytKey() As Byte, ByVal bytIV() As Byte, ByVal Direction As CryptoAction)

        Try 'In case of errors.

            'Setup file streams to handle input and output.
            fsInput = New System.IO.FileStream(strInputFile, FileMode.Open, FileAccess.Read)
            fsOutput = New System.IO.FileStream(strOutputFile, FileMode.OpenOrCreate, FileAccess.Write)
            fsOutput.SetLength(0) 'make sure fsOutput is empty

            'Declare variables for encrypt/decrypt process.
            Dim bytBuffer(4096) As Byte 'holds a block of bytes for processing
            Dim lngBytesProcessed As Long = 0 'running count of bytes processed
            Dim lngFileLength As Long = fsInput.Length 'the input file's length
            Dim intBytesInCurrentBlock As Integer 'current bytes being processed
            Dim csCryptoStream As CryptoStream
            'Declare your CryptoServiceProvider.
            Dim cspRijndael As New System.Security.Cryptography.RijndaelManaged


            'Determine if ecryption or decryption and setup CryptoStream.
            Select Case Direction
                Case CryptoAction.ActionEncrypt
                    csCryptoStream = New CryptoStream(fsOutput, cspRijndael.CreateEncryptor(bytKey, bytIV), CryptoStreamMode.Write)

                Case CryptoAction.ActionDecrypt
                    csCryptoStream = New CryptoStream(fsOutput, cspRijndael.CreateDecryptor(bytKey, bytIV), CryptoStreamMode.Write)
            End Select

            'Use While to loop until all of the file is processed.
            While lngBytesProcessed < lngFileLength
                'Read file with the input filestream.
                intBytesInCurrentBlock = fsInput.Read(bytBuffer, 0, 4096)
                'Write output file with the cryptostream.
                csCryptoStream.Write(bytBuffer, 0, intBytesInCurrentBlock)
                'Update lngBytesProcessed
                lngBytesProcessed = lngBytesProcessed + CLng(intBytesInCurrentBlock)

            End While

            'Close FileStreams and CryptoStream.
            csCryptoStream.Close()
            fsInput.Close()
            fsOutput.Close()

            'If encrypting then delete the original unencrypted file.
            If Direction = CryptoAction.ActionEncrypt Then
                Dim fileOriginal As New FileInfo(strFileToEncrypt)
                fileOriginal.Delete()
            End If

            'If decrypting then delete the encrypted file.
            If Direction = CryptoAction.ActionDecrypt Then
                Dim fileEncrypted As New FileInfo(strFileToDecrypt)
                fileEncrypted.Delete()
            End If

            'Update the user when the file is done.
            Dim Wrap As String = Chr(13) + Chr(10)
            If Direction = CryptoAction.ActionEncrypt Then
                MsgBox("Encryption Complete" + Wrap + Wrap + "Total bytes processed = " + lngBytesProcessed.ToString, MsgBoxStyle.Information, "Done")
            Else
                'Update the user when the file is done.
                MsgBox("Decryption Complete" + Wrap + Wrap + "Total bytes processed = " + lngBytesProcessed.ToString, MsgBoxStyle.Information, "Done")

            End If


            'Catch file not found error.
        Catch When Err.Number = 53 'if file not found
            MsgBox("Please check to make sure the path and filename are correct and if the file exists.", MsgBoxStyle.Exclamation, "Invalid Path or Filename")

            'Catch all other errors. And delete partial files.
        Catch
            fsInput.Close()
            fsOutput.Close()

            If Direction = CryptoAction.ActionDecrypt Then
                Dim fileDelete As New FileInfo(txtDestinationDecrypt)
                fileDelete.Delete()
                MsgBox("Please check to make sure that you entered the correct password.", MsgBoxStyle.Exclamation, "Invalid Password")
            Else
                Dim fileDelete As New FileInfo(txtDestinationEncrypt)
                fileDelete.Delete()
                MsgBox("This file cannot be encrypted.", MsgBoxStyle.Exclamation, "Invalid File")

            End If

        End Try
    End Sub

    Dim txtDestinationDecrypt, txtDestinationEncrypt As String
    Dim txtFileToEncrypt As String
    '******************************
    '** Browse/Change Buttons
    '******************************
    Public Sub BrowseEncrypt(ByVal strFileToEncrypt As String, txtConPassEncrypt As String, txtPassEncrypt As String)
        strFileToEncrypt = strFileToEncrypt
        txtFileToEncrypt = strFileToEncrypt

        Dim iPosition As Integer = 0
        Dim i As Integer = 0

        'Get the position of the last "\" in the OpenFileDialog.FileName path.
        '-1 is when the character your searching for is not there.
        'IndexOf searches from left to right.
        While strFileToEncrypt.IndexOf("\"c, i) <> -1
            iPosition = strFileToEncrypt.IndexOf("\"c, i)
            i = iPosition + 1
        End While
        'Assign strOutputFile to the position after the last "\" in the path.
        'This position is the beginning of the file name.
        strOutputEncrypt = strFileToEncrypt.Substring(iPosition + 1)
        'Assign S the entire path, ending at the last "\".
        Dim S As String = strFileToEncrypt.Substring(0, iPosition + 1)
        'Replace the "." in the file extension with "_".
        strOutputEncrypt = strOutputEncrypt.Replace("."c, "_"c)
        'The final file name.  XXXXX.encrypt
        txtDestinationEncrypt = S + strOutputEncrypt + ".encrypt"
        EncryptFile(txtConPassEncrypt, txtPassEncrypt)

    End Sub
    Dim txtFileToDecrypt As String
    Public Sub BrowseDecrypt(ByVal strFileToDecrypt As String)

        txtFileToDecrypt = strFileToDecrypt
            Dim iPosition As Integer = 0
            Dim i As Integer = 0
        'Get the position of the last "\" in the OpenFileDialog.FileName path.
        '-1 is when the character your searching for is not there.
        'IndexOf searches from left to right.

        While strFileToDecrypt.IndexOf("\"c, i) <> -1
            iPosition = strFileToDecrypt.IndexOf("\"c, i)
                i = iPosition + 1
        End While

        'strOutputFile = the file path minus the last 8 characters (.encrypt)
        strOutputDecrypt = strFileToDecrypt.Substring(0, strFileToDecrypt.Length - 8)
        'Assign S the entire path, ending at the last "\".
        Dim S As String = strFileToDecrypt.Substring(0, iPosition + 1)
        'Assign strOutputFile to the position after the last "\" in the path.
        strOutputDecrypt = strOutputDecrypt.Substring((iPosition + 1))
        'Replace "_" with "."
        txtDestinationDecrypt = S + strOutputDecrypt.Replace("_"c, "."c)

    End Sub

    Public Sub ChangeEncryptDestination(ByVal folderPath As String)
        'Setup up folder browser.
        Dim FolderBrowserDialog As New FolderBrowserDialog
        FolderBrowserDialog.Description = "Select a folder to place the encrypted file in."
        'If the user selected a folder assign the path to txtDestinationEncrypt.
        If FolderBrowserDialog.ShowDialog = DialogResult.OK Then
            txtDestinationEncrypt = FolderBrowserDialog.SelectedPath + "\" + strOutputEncrypt + ".encrypt"
        End If
    End Sub

    Public Sub ChangeDecryptDestination(ByVal folderPath As String)
        Dim FolderBrowserDialog As New FolderBrowserDialog
        'Setup up folder browser.
        FolderBrowserDialog.Description = "Select a folder for to place the decrypted file in."
        'If the user selected a folder assign the path to txtDestinationDecrypt.
        If FolderBrowserDialog.ShowDialog = DialogResult.OK Then
            txtDestinationDecrypt = FolderBrowserDialog.SelectedPath + "\" + strOutputDecrypt.Replace("_"c, "."c)
        End If
    End Sub

    '******************************
    '** Encrypt/Decrypt Buttons
    '******************************
    Public Sub EncryptFile(txtConPassEncrypt As String, txtPassEncrypt As String)
        'Make sure the password is correct.
        If txtConPassEncrypt = txtPassEncrypt Then
            'Declare variables for the key and iv.
            'The key needs to hold 256 bits and the iv 128 bits.
            Dim bytKey As Byte()
            Dim bytIV As Byte()
            'Send the password to the CreateKey function.
            bytKey = CreateKey(txtPassEncrypt)
            'Send the password to the CreateIV function.
            bytIV = CreateIV(txtPassEncrypt)
            'Start the encryption.
            EncryptOrDecryptFile(strFileToEncrypt, txtDestinationEncrypt, bytKey, bytIV, CryptoAction.ActionEncrypt)
        Else
            MsgBox("Please re-enter your password.", MsgBoxStyle.Exclamation)

        End If
    End Sub

    Public Sub DecryptFile(txtConPassDecrypt As String, txtPassDecrypt As String)
        'Make sure the password is correct.
        If txtConPassDecrypt = txtPassDecrypt Then
            'Declare variables for the key and iv.
            'The key needs to hold 256 bits and the iv 128 bits.
            Dim bytKey As Byte()
            Dim bytIV As Byte()
            'Send the password to the CreateKey function.
            bytKey = CreateKey(txtPassDecrypt)
            'Send the password to the CreateIV function.
            bytIV = CreateIV(txtPassDecrypt)
            'Start the decryption.
            EncryptOrDecryptFile(strFileToDecrypt, txtDestinationDecrypt, bytKey, bytIV, CryptoAction.ActionDecrypt)
        Else
            MsgBox("Please re-enter your password.", MsgBoxStyle.Exclamation)

        End If
    End Sub
End Class
