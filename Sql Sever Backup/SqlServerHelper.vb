Imports System.Data.SqlClient
Imports System.Collections.Generic

Public Class SqlServerHelper
    Public connectionString As String = "Data Source=localhost;Initial Catalog=master;Integrated Security=True"
    Private conn As SqlConnection
    Public Function GetSqlConnectionString(server As String, Optional database As String = "", Optional username As String = "", Optional password As String = "") As String
        ' Build the connection string
        Dim connectionStringBuilder As New SqlConnectionStringBuilder()
        ' Set the server
        connectionStringBuilder.DataSource = server
        ' Set the database if provided
        If Not String.IsNullOrEmpty(database) Then
            connectionStringBuilder.InitialCatalog = database
        Else
            connectionStringBuilder.InitialCatalog = "master"
        End If

        ' Set the username and password if provided
        If Not String.IsNullOrEmpty(username) Then
            connectionStringBuilder.UserID = username
        Else
            ' Use Windows authentication (Integrated Security=True)
            connectionStringBuilder.IntegratedSecurity = True
        End If

        If Not String.IsNullOrEmpty(password) Then
            connectionStringBuilder.Password = password
        End If
        ' Add other optional settings if needed
        connectionStringBuilder.ConnectTimeout = 30
        ' Return the connection string
        Return connectionStringBuilder.ToString()
    End Function

    'Public Sub New(conf As String)
    '    If conf IsNot Nothing AndAlso Not String.IsNullOrEmpty(conf.ToString()) Then
    '        If TestConnection(conf) Then
    '            connectionString = conf.ToString()
    '        End If
    '    End If
    'End Sub

    Public Function TestConnection(conn As String) As Boolean
        Try
            Using testConn As New SqlConnection(conn)
                testConn.Open()
                If testConn.State = ConnectionState.Open Then
                    testConn.Close()
                    Return True
                Else
                    Return False
                End If
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error testing connection: {ex.Message}")
            Return False
        End Try
    End Function

    ' Add an event handler to handle InfoMessage events
    Private Sub OnInfoMessage(sender As Object, e As SqlInfoMessageEventArgs)
        For Each message As SqlError In e.Errors
            ' Display or log the messages as needed
            Console.WriteLine($"SQL Server Message: {message.Message}")
        Next
    End Sub

    Public Function GetAllDatabaseNames() As List(Of String)
        Dim databaseNames As New List(Of String)()

        Try
            Using conn As New SqlConnection(connectionString)
                conn.Open()
                ' Query to get all database names
                Dim query As String = "SELECT name FROM sys.databases"

                Using cmd As New SqlCommand(query, conn)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            databaseNames.Add(reader("name").ToString())
                        End While
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Console.WriteLine($"Error getting database names: {ex.Message}")
        End Try

        Return databaseNames
    End Function

    Public Sub Exec(ByVal StrSQL As String, Optional ByVal iTimeout As Integer = 180)
        Dim StringQuery As String = StrSQL
        Dim QryInsert As New SqlCommand

        Try
            With conn
                If .State = ConnectionState.Open Then
                    .Close()
                    .ConnectionString = connectionString
                    .Open()
                Else
                    .ConnectionString = connectionString
                    .Open()
                End If
            End With

            ' Register the event handler for InfoMessage
            AddHandler conn.InfoMessage, AddressOf OnInfoMessage

            With QryInsert
                .CommandTimeout = iTimeout 'timeout S
                .CommandType = CommandType.Text
                .CommandText = StrSQL
                .Connection = conn
                .ExecuteNonQuery()
            End With

            conn.Close()

        Catch ex As Exception
            ' Handle exceptions
            Console.WriteLine($"Error executing query: {ex.Message}")

        Finally
            ' Unregister the event handler
            RemoveHandler conn.InfoMessage, AddressOf OnInfoMessage
        End Try
    End Sub


End Class

