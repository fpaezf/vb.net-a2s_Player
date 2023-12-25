Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Public Class A2S_player


    Public Shared Sub A2S_player()
        'This is the A2S_Player packet in Hex: ÿÿÿÿUÿÿÿÿ
        Dim A2S_PLAYER_Request() As Byte = {&HFF, &HFF, &HFF, &HFF, &H55, &HFF, &HFF, &HFF, &HFF}

        'Declare the target server as IPEndpoint IP/PORT
        Dim targetServer As New IPEndPoint(IPAddress.Parse("123.123.123.123"), 27015)

        'Declare UDPClient
        Using client As New UdpClient

            'Timeout settings
            client.Client.ReceiveTimeout = 2000
            client.Client.SendTimeout = 2000

            'Send A2S_Player packet to target server
            client.Send(A2S_PLAYER_Request, A2S_PLAYER_Request.Length, targetServer)

            'Receive response and parse Challenge bytes
            Dim Challenge_Request As Byte() = client.Receive(targetServer)
            Dim Challenge_Bytes As Byte() = {Challenge_Request(5), Challenge_Request(6), Challenge_Request(7), Challenge_Request(8)}

            'Build the response to challenge request:  A2S_PLAYER_Request + Challenge bytes
            Dim A2S_Request_With_Challenge_Bytes As Byte() = {&HFF, &HFF, &HFF, &HFF, &H55, Challenge_Bytes(0), Challenge_Bytes(1), Challenge_Bytes(2), Challenge_Bytes(3)}

            'Send the challenge request response
            client.Send(A2S_Request_With_Challenge_Bytes, A2S_Request_With_Challenge_Bytes.Length, targetServer)

            'Receive the response from server
            Dim A2S_Player As Byte() = client.Receive(targetServer)

            'Put server response in a memory stream And start a binary reader to read the stream
            Dim stream As MemoryStream = New MemoryStream(A2S_Player)
            Dim reader As BinaryReader = New BinaryReader(stream)

            'Skip first 4 bytes
            stream.Seek(4, SeekOrigin.Begin)

            'Header has to be -1, if not, bad response from server
            Dim header As Integer = reader.ReadByte()

            'Number of players in the server
            Dim PlayerCounter As Integer = reader.ReadByte()

            'For each player in the server...
            For i = 0 To PlayerCounter - 1
                Dim index As Integer = reader.ReadByte()
                Dim Name As String = ReadSteamString(reader)
                Dim Score As Integer = reader.ReadInt32()
                Dim Duration As Single = reader.ReadSingle()
                'Show player data
                MsgBox(index & ":" & Name & ":" & Score.ToString & ":" & Duration.ToString)
            Next
        End Using
    End Sub

    'Function to read null terminated strings with binary reader
    Public Shared Function ReadSteamString(ByVal reader As BinaryReader) As String
        Dim str As List(Of Byte) = New List(Of Byte)()
        Dim nextByte As Byte = reader.ReadByte()

        While nextByte <> 0
            str.Add(nextByte)
            nextByte = reader.ReadByte()
        End While

        Return Encoding.UTF8.GetString(str.ToArray())
    End Function

End Class
