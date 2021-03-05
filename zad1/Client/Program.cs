using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Text.Encoding;

const int port = 3000;
var serverEndPoint = new IPEndPoint(IPAddress.Loopback, port);

Console.Write("Enter username: ");
var username = Console.ReadLine()!;

var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
tcpSocket.Connect(serverEndPoint);

var localPort = ((IPEndPoint) tcpSocket.LocalEndPoint!).Port;
var udpEndPoint = new IPEndPoint(IPAddress.Loopback, localPort);

udpSocket.Bind(udpEndPoint);

Console.WriteLine("Connected with server");

var netStream = new NetworkStream(tcpSocket, true);
var writer = new BinaryWriter(netStream);
var reader = new BinaryReader(netStream);

writer.Write(username);
writer.Flush();

void HandleUdpMessage() {
    var buffer = new byte[1024];
    while (true) {
        EndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        var len = udpSocket!.ReceiveFrom(buffer, ref endPoint);
        var message = UTF8.GetString(buffer, 0, len);
        Console.WriteLine(message);
    }
}

void HandleTcpMessage() {
    while (true) {
        var message = reader!.ReadString();
        Console.WriteLine(message);
    }
}

Task.Factory.StartNew(HandleTcpMessage);
Task.Factory.StartNew(HandleUdpMessage);

while (true) {
    Console.Write($"{username}> ");
    var message = Console.ReadLine()!;
    if (message == "U") { // send UDP UniCast message
        var payload = UTF8.GetBytes("This message is send via UDP datagram UniCast");
        udpSocket.SendTo(payload, serverEndPoint);
    } else {
        writer.Write(message);
        writer.Flush();
    }
}
