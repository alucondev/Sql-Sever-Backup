Imports System.IO
Imports System.Security.Cryptography

Public Class FileEncryptor
    Private strFileToEncrypt As String
    Private strFileToDecrypt As String
    Private strOutputEncrypt As String
    Private strOutputDecrypt As String
    Private fsInput As FileStream
    Private fsOutput As FileStream

    Public Enum CryptoAction
        ActionEncrypt = 1
        ActionDecrypt = 2
    End Enum

    Private Function CreateKey(ByVal strPassword As String) As Byte()
        Dim chrData() As Char = strPassword.ToCharArray
        Dim intLength As Integer = chrData.GetUpperBound(0)
        Dim bytDataToHash(intLength) As Byte

        For i As Integer = 0 To chrData.GetUpperBound(0)
            bytDataToHash(i) = CByte(Asc(chrData(i)))
        Next

        Dim SHA512 As New System.Security.Cryptography.SHA512Managed
        Dim bytResult As Byte() = SHA512.ComputeHash(bytDataToHash)
        Dim bytKey(31) As Byte

        For i As Integer = 0 To 31
            bytKey(i) = bytResult(i)
        Next

        Return bytKey
    End Function

    Private Function CreateIV(ByVal strPassword As String) As Byte()
        Dim chrData() As Char = strPassword.ToCharArray
        Dim intLength As Integer = chrData.GetUpperBound(0)
        Dim bytDataToHash(intLength) As Byte

        For i As Integer = 0 To chrData.GetUpperBound(0)
            bytDataToHash(i) = CByte(Asc(chrData(i)))
        Next
        Dim SHA512 As New System.Security.Cryptography.SHA512Managed
        Dim bytResult As Byte() = SHA512.ComputeHash(bytDataToHash)
        Dim bytIV(15) As Byte

        For i As Integer = 32 To 47
            bytIV(i - 32) = bytResult(i)
        Next

        Return bytIV
    End Function

    Private Sub EncryptOrDecryptFile(ByVal strInputFile As String, ByVal strOutputFile As String, ByVal bytKey() As Byte, ByVal bytIV() As Byte, ByVal Direction As CryptoAction)
        Try
            fsInput = New FileStream(strInputFile, FileMode.Open, FileAccess.Read)
            fsOutput = New FileStream(strOutputFile, FileMode.OpenOrCreate, FileAccess.Write)
            fsOutput.SetLength(0)

            Dim bytBuffer(4096) As Byte
            Dim lngBytesProcessed As Long = 0
            Dim lngFileLength As Long = fsInput.Length
            Dim intBytesInCurrentBlock As Integer
            Dim csCryptoStream As CryptoStream
            Dim cspRijndael As New System.Security.Cryptography.RijndaelManaged

            Select Case Direction
                Case CryptoAction.ActionEncrypt
                    csCryptoStream = New CryptoStream(fsOutput, cspRijndael.CreateEncryptor(bytKey, bytIV), CryptoStreamMode.Write)
                Case CryptoAction.ActionDecrypt
                    csCryptoStream = New CryptoStream(fsOutput, cspRijndael.CreateDecryptor(bytKey, bytIV), CryptoStreamMode.Write)
            End Select

            While lngBytesProcessed < lngFileLength
                intBytesInCurrentBlock = fsInput.Read(bytBuffer, 0, 4096)
                csCryptoStream.Write(bytBuffer, 0, intBytesInCurrentBlock)
                lngBytesProcessed += CLng(intBytesInCurrentBlock)
            End While

            ' Ensure that CryptoStream and FileStream are closed
            csCryptoStream.Close()
            fsInput.Close()
            fsOutput.Close()

            If Direction = CryptoAction.ActionEncrypt Then
                ' Check for file existence before deletion
                If File.Exists(strFileToEncrypt) Then
                    Dim fileOriginal As New FileInfo(strFileToEncrypt)
                    fileOriginal.Delete()
                End If
            End If

            If Direction = CryptoAction.ActionDecrypt Then
                ' Check for file existence before deletion
                If File.Exists(strFileToDecrypt) Then
                    Dim fileEncrypted As New FileInfo(strFileToDecrypt)
                    fileEncrypted.Delete()
                End If
            End If

            Dim Wrap As String = Chr(13) + Chr(10)
            If Direction = CryptoAction.ActionEncrypt Then
                MessageBox.Show("Encryption Complete" + Wrap + Wrap + "Total bytes processed = " + lngBytesProcessed.ToString, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show("Decryption Complete" + Wrap + Wrap + "Total bytes processed = " + lngBytesProcessed.ToString, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            ' Handle exceptions here
            MessageBox.Show(ex.Message, "Error during encryption/decryption: ")
        Finally
            ' Ensure that FileStreams are closed in case of an exception
            If fsInput IsNot Nothing Then fsInput.Close()
            If fsOutput IsNot Nothing Then fsOutput.Close()
        End Try
    End Sub

    Public Sub EncryptFile(ByVal inputFile As String, ByVal outputFile As String, ByVal password As String)
        strFileToEncrypt = inputFile
        strOutputEncrypt = outputFile

        Dim bytKey As Byte() = CreateKey(password)
        Dim bytIV As Byte() = CreateIV(password)

        EncryptOrDecryptFile(strFileToEncrypt, strOutputEncrypt, bytKey, bytIV, CryptoAction.ActionEncrypt)
    End Sub

    Public Sub DecryptFile(ByVal inputFile As String, ByVal outputFile As String, ByVal password As String)
        strFileToDecrypt = inputFile
        strOutputDecrypt = outputFile

        Dim bytKey As Byte() = CreateKey(password)
        Dim bytIV As Byte() = CreateIV(password)

        EncryptOrDecryptFile(strFileToDecrypt, strOutputDecrypt, bytKey, bytIV, CryptoAction.ActionDecrypt)
    End Sub

End Class
