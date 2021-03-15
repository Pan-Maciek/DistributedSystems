using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Server {
    public class Client {
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;
        public readonly EndPoint EndPoint;
        public readonly string Username;

        private Client(BinaryReader reader, BinaryWriter writer, EndPoint endPoint, string username) {
            _reader = reader;
            _writer = writer;
            EndPoint = endPoint;
            Username = username;
        }

        public static Client CreateFromSocket(Socket socket) {
            var netStream = new NetworkStream(socket, true);

            var reader = new BinaryReader(netStream);
            var writer = new BinaryWriter(netStream);

            var username = reader.ReadString();

            return new Client(reader, writer, socket.RemoteEndPoint!, username);
        }

        public string ReadMessage() => _reader.ReadString();

        public void SendMessage(string message) => _writer.Write(message);
    }
}