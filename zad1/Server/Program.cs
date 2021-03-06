using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Server;
using static System.Text.Encoding;

const int port = 3000;
var serverEndPoint = new IPEndPoint(IPAddress.Loopback, port);

var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

udpSocket.Bind(serverEndPoint);
tcpSocket.Bind(serverEndPoint);
tcpSocket.Listen();

var clients = new List<Client>();
void HandleNewConnection(Socket socket) {
    var client = Client.CreateFromSocket(socket);
    lock (clients!) {
        clients.Add(client);
    }

    while (true) {
        var message = client.ReadMessage();
        lock (clients) {
            foreach (var c in clients) {
                if (c.EndPoint.Equals(client.EndPoint)) continue;
                c.SendMessage($"{client.Username}> {message}");
            }
        }
    }
}

void HandleUdpMessages() {
    var buffer = new byte[1024];
    while (true) {
        EndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        var len = udpSocket.ReceiveFrom(buffer, ref endPoint);
        var username = (from c in clients
                        where c.EndPoint.Equals(endPoint)
                        select c.Username).First();
        var message = $"{username}> {UTF8.GetString(buffer, 0, len)}";
        var payload = UTF8.GetBytes(message);
        lock (clients) {
            foreach (var c in clients) {
                if (c.EndPoint.Equals(endPoint)) continue;
                udpSocket.SendTo(payload, c.EndPoint);
            }
        }
    }
}

void AcceptTcpConnections() {
    while (true) {
        var clientSocket = tcpSocket.Accept();
        Task.Factory.StartNew(() => HandleNewConnection(clientSocket));
    }
}

var tasks = new[] {
    Task.Factory.StartNew(HandleUdpMessages),
    Task.Factory.StartNew(AcceptTcpConnections)
};
Console.WriteLine($"Server listening at {serverEndPoint}");
Task.WaitAll(tasks);