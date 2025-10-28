using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Uge44ProjektOpgaveNewsletter.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private TcpClient? _client;
        private string? _responseData;

        public Window1()
        {
            InitializeComponent();
        }
        public void SetConnection(System.IO.StreamReader? reader, System.IO.StreamWriter? writer, System.Net.Sockets.TcpClient? client, string? responseData)
        {

            _reader = reader;
            _writer = writer;
            _client = client;
            _responseData = responseData;

        }

        private void savedGroupsLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void lastUsedCommandLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void confirmCommandoButton_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
