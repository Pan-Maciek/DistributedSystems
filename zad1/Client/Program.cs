using System;
using System.Net;
using System.Net.Sockets;

const int port = 3000;

Console.Write("Enter username: ");
var username = Console.ReadLine()!;

using var tcpClient = new TcpClient();
tcpClient.Connect(IPAddress.Loopback, port);

Console.WriteLine("Connected with server");