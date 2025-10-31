using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uge44ProjektOpgaveNewsletter.Models;
using Uge44ProjektOpgaveNewsletter.Service;

namespace Uge44ProjektOpgaveNewsletter.Views
{
    public partial class CommandWindow : Window
    {
        private readonly ConnectionService _connection = ConnectionService.Instance;
        private GroupService _groupService = new GroupService();
        private string? _responseData;
        private string? _currentGroup;
        private string? _User;

        public CommandWindow()
        {
            InitializeComponent();

            // Fill command list once
            lastUsedCommandLV.Items.Clear();
            foreach (CommandWindowCommandTypes command in Enum.GetValues(typeof(CommandWindowCommandTypes)))
                lastUsedCommandLV.Items.Add(command);
        }

        public void SetServerAndUser(string serverName, string userName)
        {
            serverNameLable.Content = serverName;
            _User = userName;

            // Load user's saved groups
            savedGroupsLV.Items.Clear();
            var groups = _groupService.LoadGroups(_User);

            if (groups.Count == 0)
            {
                savedGroupsLV.Items.Add("(No saved groups yet)");
            }
            else
            {
                foreach (var g in groups)
                    savedGroupsLV.Items.Add(g);
            }
        }

        // --- Command selection logic ---
        private void lastUsedCommandLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lastUsedCommandLV.SelectedItem != null)
                commandotTextbox.Text = lastUsedCommandLV.SelectedItem.ToString();
        }

        // --- Confirm Command ---
        private async void confirmCommandoButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(commandotTextbox.Text))
            {
                MessageBox.Show("Please enter a command.");
                return;
            }

            string command = commandotTextbox.Text.Trim();
            InfoTextBlock.Text = "Processing command...";

            string result = await Task.Run(() => SendCommand(command));
            InfoTextBlock.Text = result;

            // Update last used commands
            if (lastUsedCommandLV.Items.Contains(command) && command != "GROUP")
            {
                lastUsedCommandLV.Items.Remove(command);
                lastUsedCommandLV.Items.Insert(0, command);
            }
            else
            {
                lastUsedCommandLV.Items.Insert(0, command);
            }
        }

        // --- Send Command ---
        private string SendCommand(string commando)
        {
            var client = _connection.Client;
            var writer = _connection.Writer;
            var reader = _connection.Reader;

            if (client == null || writer == null || reader == null || !client.Connected)
                return "Not connected to any server.";

            try
            {
                client.ReceiveTimeout = 15000;
                client.SendTimeout = 15000;

                // --- POST ---
                if (commando.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    if (_currentGroup == null)
                        return "Please select a newsgroup first using the GROUP command.";

                    writer.WriteLine("POST");
                    writer.Flush();

                    string? response = reader.ReadLine();
                    if (response == null || !response.StartsWith("340"))
                        return $"Server rejected POST: {response}";

                    // Show PostWindow safely on UI thread
                    bool? dialogResult = null;
                    string subject = "";
                    string body = "";

                    Dispatcher.Invoke(() =>
                    {
                        var postWindow = new PostWindow { Owner = this };
                        dialogResult = postWindow.ShowDialog();
                        if (dialogResult == true)
                        {
                            subject = postWindow.PostSubject;
                            body = postWindow.PostBody;
                        }
                    });

                    if (dialogResult != true)
                        return "Post cancelled.";

                    // Build message
                    List<string> articleLines = new List<string>
                    {
                        $"From: {_User}",
                        $"Newsgroups: {_currentGroup}",
                        $"Subject: {subject}",
                        "Content-Type: text/plain; charset=\"UTF-8\"",
                        "Content-Transfer-Encoding: 8bit",
                        $"Date: {DateTime.UtcNow:R}",
                        ""
                    };
                    articleLines.AddRange(body.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));

                    foreach (var line in articleLines)
                        writer.WriteLine(line);

                    writer.WriteLine("."); // End of message
                    writer.Flush();

                    string? postResponse = reader.ReadLine();
                    return postResponse is not null && postResponse.StartsWith("240")
                        ? " Article posted successfully!"
                        : $" Post failed: {postResponse}";
                }

                // --- GROUP ---
                if (commando.StartsWith("GROUP", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteLine(commando);
                    writer.Flush();

                    string? line = reader.ReadLine();
                    if (line == null)
                        return "No response from server for GROUP command.";

                    var parts = line.Split(' ');
                    if (parts.Length >= 5)
                        _currentGroup = parts[4];

                    return line.FormatGroupResponse();
                }

                // --- Other commands (LIST, XOVER, ARTICLE, etc.) ---
                writer.WriteLine(commando);
                writer.Flush();

                StringBuilder responseData = new StringBuilder();
                string? serverLine;

                while ((serverLine = reader.ReadLine()) != null)
                {
                    if (serverLine.StartsWith("4") || serverLine.StartsWith("5"))
                    {
                        responseData.AppendLine($"Server error: {serverLine}");
                        break;
                    }

                    if (serverLine == ".")
                        break;

                    responseData.AppendLine(serverLine);
                }

                string raw = responseData.ToString();
                _responseData = raw;

                if (commando.StartsWith("XOVER", StringComparison.OrdinalIgnoreCase))
                    return raw.FormatXoverResponse();

                if (commando.StartsWith("ARTICLE", StringComparison.OrdinalIgnoreCase))
                    return raw.FormatXoverResponse();

                return raw;
            }
            catch (IOException ex)
            {
                return $"Network error: {ex.Message}";
            }
            catch (SocketException ex)
            {
                return $"Connection error: {ex.Message}";
            }
        }

        // --- Save group button to file ---
        private void SaveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(commandotTextbox.Text))
            {
                MessageBox.Show("Please enter a GROUP command first.");
                return;
            }

            string commandText = commandotTextbox.Text.Trim();

            if (!commandText.StartsWith("GROUP "))
            {
                MessageBox.Show("GROUP command must start with 'GROUP '.");
                return;
            }

            string groupName = commandText.Substring(6).Trim();

            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("No group name specified.");
                return;
            }

            _groupService.SaveGroup(_User, groupName);

            if (!savedGroupsLV.Items.Contains(groupName))
                savedGroupsLV.Items.Add(groupName);

            MessageBox.Show($"Group '{groupName}' saved for user {_User}.");
        }
        // --- Saved group selection ---
        private void savedGroupsLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (savedGroupsLV.SelectedItem is not null)
            {
                commandotTextbox.Text = "GROUP " + savedGroupsLV.SelectedItem.ToString();
            }
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_responseData))
            {
                MessageBox.Show("No response data to search. Please execute a command first.");
                return;
            }
            string searchTerm = SearchTextbox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Please enter a search term.");
                return;
            }
            StringBuilder searchResults = new StringBuilder();
            string[] lines = _responseData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    searchResults.AppendLine(line);
                }
            }
            if (searchResults.Length == 0)
            {
                InfoTextBlock.Text = "No matches found.";
            }
            else
            {
                InfoTextBlock.Text = $"Search results for '{searchTerm}':\n{searchResults}";
            }
        }
    }
}
