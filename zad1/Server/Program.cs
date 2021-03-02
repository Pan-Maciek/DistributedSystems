using System;
using System.Net;
using System.Net.Sockets;

const int port = 3000;
var serverEndPoint = new IPEndPoint(IPAddress.Loopback, port);

var tcpListener = new TcpListener(serverEndPoint);
tcpListener.Start();

Console.WriteLine($"Server listening at {serverEndPoint}");
while (true)
{
    var tcpClient = tcpListener.AcceptTcpClient();
    Console.WriteLine("Accepted tcp client");
}