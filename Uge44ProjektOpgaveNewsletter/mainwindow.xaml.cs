using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uge44ProjektOpgaveNewsletter.Models;
using Uge44ProjektOpgaveNewsletter.Service;
using Uge44ProjektOpgaveNewsletter.Views;

namespace Uge44ProjektOpgaveNewsletter
{
    public partial class MainWindow : Window
    {
        private readonly SaveUserService savedUsers = new SaveUserService();
        private readonly string userFilePath = "SavedUsers.txt";
        private readonly ConnectionService connection = ConnectionService.Instance;

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
                string response = await connection.ConnectAsync(host, port);
                responseTextbox.Text += $"Server response:\n{response}\nConnected to {host}:{port}\n";
            }
            catch (Exception ex)
            {
                responseTextbox.Text += $"Error connecting: {ex.Message}\n";
            }
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!connection.IsConnected)
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

            responseTextbox.Text += "Sending login info...\n";

            try
            {
                string userResp = await connection.SendCommandAsync($"AuthInfo user {username}");
                responseTextbox.Text += $"Server: {userResp}\n";

                if (!userResp.StartsWith("381"))
                {
                    responseTextbox.Text += "Username not accepted by server.\n";
                    return;
                }

                string passResp = await connection.SendCommandAsync($"AuthInfo pass {password}");
                responseTextbox.Text += $"Server: {passResp}\n";

                if (passResp.StartsWith("281"))
                {
                    responseTextbox.Text += "Login successful!\n";
                    OpenCommandWindow();
                }
                else
                {
                    responseTextbox.Text += "Login failed. Check your credentials.\n";
                }
            }
            catch (Exception ex)
            {
                responseTextbox.Text += "Error sending login: " + ex.Message + "\n";
            }
        }

        private void saveLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = userNameTextbox.Text.Trim();
            string password = passwordBox.Password.Trim();
            string serverName = serverNameTextbox.Text.Trim();
            string port = portTextbox.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Enter both username and password to save.");
                return;
            }

            savedUsers.SaveUser(username, password, serverName, port, userFilePath);
            MessageBox.Show("Login saved successfully.");

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
                serverNameTextbox.Text = savedUsers.GetServerName(selectedUsername);
                portTextbox.Text = savedUsers.GetPort(selectedUsername);
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            savedUsers.LoadUsers(userFilePath);
            savedUserCB.ItemsSource = savedUsers.GetUsernames();
        }

        private void OpenCommandWindow()
        {
            var commandWindow = new Views.CommandWindow();
            commandWindow.SetServerAndUser(serverNameTextbox.Text, userNameTextbox.Text);
            commandWindow.Show();
            this.Close();
        }
    }
}
