using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Uge44ProjektOpgaveNewsletter.Service
{
    public sealed class ConnectionService
    {
        private static readonly Lazy<ConnectionService> _instance =
            new Lazy<ConnectionService>(() => new ConnectionService());

        public static ConnectionService Instance => _instance.Value;

        private TcpClient? _client;
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private string? _responseData;

        private ConnectionService() { }

        public bool IsConnected => _client != null && _client.Connected;

        public async Task<string> ConnectAsync(string host, int port)
        {
            _client = new TcpClient();
            _client.ReceiveTimeout = 3000;
            _client.SendTimeout = 3000;

            await _client.ConnectAsync(host, port);

            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.ASCII);
            _writer = new StreamWriter(_stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };

            _responseData = await _reader.ReadLineAsync();
            return _responseData ?? "No response";
        }

        public async Task<string> SendCommandAsync(string command)
        {
            if (_writer == null || _reader == null)
                throw new InvalidOperationException("Not connected to server.");

            await _writer.WriteLineAsync(command);
            _responseData = await _reader.ReadLineAsync();
            return _responseData ?? "";
        }

        public string LastResponse => _responseData ?? "";

        public StreamReader? Reader => _reader;
        public StreamWriter? Writer => _writer;
        public TcpClient? Client => _client;

        public void Disconnect()
        {
            _reader?.Close();
            _writer?.Close();
            _stream?.Close();
            _client?.Close();
        }
    }
}
