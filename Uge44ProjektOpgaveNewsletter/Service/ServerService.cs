using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Uge44ProjektOpgaveNewsletter.Views;

namespace Uge44ProjektOpgaveNewsletter.Service
{
    public class ServerService
    {
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private TcpClient? _client;
        private string? _responseData;
        private string? _currentGroup;
        public ServerService()
        {
            
        }
        
    }
}
