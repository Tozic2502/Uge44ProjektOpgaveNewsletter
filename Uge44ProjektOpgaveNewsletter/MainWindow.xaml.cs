using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using Uge44ProjektOpgaveNewsletter.Models;
using Uge44ProjektOpgaveNewsletter.Service;

namespace Uge44ProjektOpgaveNewsletter
{
    public partial class MainWindow : Window
    {
        // Change these fields to nullable by adding '?'
        private TcpClient? _client;
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private string? _responseData;
        private string userFilePath = "SavedUsers.txt"; // same file path you used earlier

        // Add this property to reference your savedUsers model
        private SaveUserService savedUsers = new SaveUserService();



        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string host = serverNameTextbox.Text.Trim();

            if (!int.TryParse(portTextbox.Text, out int port))
            {
                MessageBox.Show("Invalid port number.");
                return;
            }

            responseTextbox.Text = "Connecting to server...\n";

            try
            {
                // Connect in a background thread
                await Task.Run(() =>
                {
                    _client = new TcpClient();
                    _client.ReceiveTimeout = 3000;
                    _client.SendTimeout = 3000;
                    _client.Connect(host, port);

                    _stream = _client.GetStream();
                    _reader = new StreamReader(_stream, Encoding.ASCII);
                    _writer = new StreamWriter(_stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };
                    _responseData = _reader.ReadLine();
                    // UI updates must be done on the UI thread
                    Dispatcher.Invoke(() => MessageBox.Show("Server response: " + _responseData));
                });

                responseTextbox.Text += $"Connected to {host}:{port}\n";
            }
            catch (Exception ex)
            {
                responseTextbox.Text += $"Error connecting: {ex.Message}\n";
            }
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null || !_client.Connected)
            {
                MessageBox.Show("You must connect to the server first!");
                return;
            }

            string username = userNameTextbox.Text.Trim();
            string password = passwordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Enter both username and password.");
                return;
            }

            string User = $"AuthInfo user {username} ";
            string Pass = $"AuthInfo pass {password} ";
            responseTextbox.Text += "Sending login info...\n";

            try
            {
                string user = await Task.Run(() => SendLogin(User));
                responseTextbox.Text += "Server response:\n" + _responseData + "\n";
                if (!_responseData.StartsWith("381"))
                {
                    responseTextbox.Text += "Username not accepted by server.\n";
                    return;
                }
                else
                {
                    string pass = await Task.Run(() => SendLogin(Pass));
                    responseTextbox.Text += "Server response:\n" + _responseData + "\n";
                }

            }
            catch (Exception ex)
            {
                responseTextbox.Text += "Error sending login: " + ex.Message + "\n";
            }

        }

        private string SendLogin(string message)
        {
            if (_writer == null || _reader == null)
                throw new InvalidOperationException("Not connected to server.");

            _writer.WriteLine(message);

            return _responseData = _reader.ReadLine();
        }

        private void saveLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = userNameTextbox.Text.Trim();
            string password = passwordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Enter both username and password to save.");
                return;
            }

            savedUsers.SaveUser(username, password, userFilePath);

            MessageBox.Show("Login saved successfully.");

            // Refresh ComboBox after saving
            savedUsers.LoadUsers(userFilePath);
            savedUserCB.ItemsSource = savedUsers.GetUsernames();
            savedUserCB.SelectedItem = username;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (savedUserCB.SelectedItem is string selectedUsername)
            {
                userNameTextbox.Text = selectedUsername;
                passwordBox.Password = savedUsers.GetPassword(selectedUsername);
            }
        }

        // Ensure ComboBox is initialized with usernames on window load
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            savedUsers.LoadUsers(userFilePath);
            savedUserCB.ItemsSource = savedUsers.GetUsernames();
        }
    }
}
