using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
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
    public partial class CommandWindow : Window
    {
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private TcpClient? _client;
        private string? _responseData;
        private string? _currentGroup;

        public CommandWindow()
        {
            InitializeComponent();
            lastUsedCommandLV.Items.Clear();
            foreach (CommandType command in Enum.GetValues(typeof(CommandType)))
            {
                lastUsedCommandLV.Items.Add(command);
            }
        }
        // Method to set the connection details from MainWindow
        public void SetConnection(System.IO.StreamReader? reader, System.IO.StreamWriter? writer, System.Net.Sockets.TcpClient? client, string? responseData, string serverName)
        {

            _reader = reader;
            _writer = writer;
            _client = client;
            _responseData = responseData;
            serverNameLable.Content = serverName;
           
        }

        // Last used command listview selection changed event to set the command textbox text and set the last used command to the top option in the listview
        private void lastUsedCommandLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if(lastUsedCommandLV.SelectedItem != null)
            {
                commandotTextbox.Text = lastUsedCommandLV.SelectedItem.ToString();
            }
            
            
                

        }
        // Confirm command button click event and sets the InfoTextBlock text to the reader's content
        private async void confirmCommandoButton_Click(object sender, RoutedEventArgs e)
        {
            if(commandotTextbox.Text == null || commandotTextbox.Text.Trim() == "")
            {
                MessageBox.Show("Please enter a command.");
                return;
            }
            sendCommandos(commandotTextbox.Text.Trim());

            InfoTextBlock.Text = sendCommandos(commandotTextbox.Text.Trim());

            if (lastUsedCommandLV.Items.Contains(lastUsedCommandLV.SelectedItem) && commandotTextbox.Text != "GROUP")
            {
                lastUsedCommandLV.Items.Remove(lastUsedCommandLV.SelectedItem);
                lastUsedCommandLV.Items.Insert(0, commandotTextbox.Text.Trim());
            }else
            {
                lastUsedCommandLV.Items.Insert(0, commandotTextbox.Text.Trim());

            }
        }
        // Method to send commands to the server and read the response


        private string sendCommandos(string commando)
        {
            if (_writer == null || _reader == null)
                throw new InvalidOperationException("Not connected to server.");

            try
            {
                _client.ReceiveTimeout = 15000; // 15s timeout
                _client.SendTimeout = 15000;

                // --- Special handling for POST ---
                if (commando.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    if (_currentGroup == null)
                        return "Please select a newsgroup first using the GROUP command.";

                    _writer.WriteLine("POST");
                    _writer.Flush();

                    string? response = _reader.ReadLine();
                    if (response == null || !response.StartsWith("340"))
                        return $"Server rejected POST: {response}";

                    // Open popup window for subject + body
                    var postWindow = new PostWindow();
                    postWindow.Owner = this;
                    bool? result = postWindow.ShowDialog();

                    if (result != true)
                        return "Post cancelled.";

                    string subject = postWindow.PostSubject;
                    string body = postWindow.PostBody;

                    // Build article line by line
                    List<string> articleLines = new List<string>
                        {
                            "From: user@example.com",
                            $"Newsgroups: {_currentGroup}",
                            $"Subject: {subject}",
                            $"Date: {DateTime.UtcNow:R}",
                            ""
                        };
                    articleLines.AddRange(body.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));

                    foreach (var articleLine in articleLines)
                    {
                        _writer.WriteLine(articleLine);
                    }
                    // End of article
                    _writer.WriteLine(".");
                    _writer.Flush();

                    string? finalResponse = _reader.ReadLine();
                    return finalResponse ?? "No response after sending article.";
                }

                // --- Normal command handling ---
                _writer.WriteLine(commando);
                _writer.Flush();

                StringBuilder responseData = new StringBuilder();
                string? line;

                // For GROUP, read only **one line**
                if (commando.StartsWith("GROUP", StringComparison.OrdinalIgnoreCase))
                {
                    line = _reader.ReadLine();
                    if (line == null)
                        return "No response from server for GROUP command.";

                    responseData.AppendLine(line);

                    // Parse current group
                    var parts = line.Split(' ');
                    if (parts.Length >= 4)
                        _currentGroup = parts[3]; // correct index

                    // Use formatting method
                    return FormatGroupResponse(responseData.ToString().Trim());
                }

                // For multi-line responses (LIST, XOVER, etc.)
                while ((line = _reader.ReadLine()) != null)
                {
                    if (line.StartsWith("4") || line.StartsWith("5"))
                    {
                        responseData.AppendLine($"Server error: {line}");
                        break;
                    }

                    if (line == ".")
                        break;

                    responseData.AppendLine(line);
                }

                string rawResponse = responseData.ToString();

                // Apply formatting based on command
                if (commando.StartsWith("ARTICLE", StringComparison.OrdinalIgnoreCase))
                {
                    return FormatArticleResponse(rawResponse);
                }

                if (commando.StartsWith("XOVER", StringComparison.OrdinalIgnoreCase))
                {
                    return FormatXoverResponse(rawResponse);
                }

                // For LIST or other commands, just return raw
                _responseData = rawResponse;
                return _responseData;
            }
            catch (IOException ex)
            {
                _responseData = $"Network error: {ex.Message}";
                return _responseData;
            }
            catch (SocketException ex)
            {
                _responseData = $"Connection error: {ex.Message}";
                return _responseData;
            }
        }




        // Save group button click event to save the group command to the saved groups listview
        private void SaveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (commandotTextbox.Text == null || commandotTextbox.Text.Trim() == "")
            {
                MessageBox.Show("Please enter a group name to save.");
                return;
            }
            if (savedGroupsLV.Items.Contains(commandotTextbox.Text.Trim()))
            {
                MessageBox.Show("This group is already saved.");
                return;
            }
            if(commandotTextbox.Text.Trim().StartsWith("GROUP "))
            {
                savedGroupsLV.Items.Add(commandotTextbox.Text.Trim());
            }else
            {
                MessageBox.Show("Please enter a valid GROUP command to save. Or make Sure GROUP is written in caps");
            }
            
        }
        // Saved groups listview selection changed event to set the command textbox text
        private void savedGroupsLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_client != null)
            {
                if (savedGroupsLV.SelectedItem != null)
                {
                    commandotTextbox.Text = "GROUP " + savedGroupsLV.SelectedItem.ToString();
                }

            }
        }
        private string FormatGroupResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return "No response received.";

            var parts = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5 || parts[0] != "211")
                return response; // Not a standard GROUP response

            string articleCount = parts[1];
            string firstArticle = parts[2];
            string lastArticle = parts[3];
            string groupName = parts[4];

            return $"Group Selected: {groupName}\n" +
                   $"Articles Available: {articleCount}\n" +
                   $"First Article #: {firstArticle}\n" +
                   $"Last Article #: {lastArticle}";
        }
        private string FormatArticleResponse(string response)
        {
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
                return "No article data.";

            StringBuilder sb = new StringBuilder();

            // Skip the first status line (e.g., "220 ...")
            sb.AppendLine("=== ARTICLE HEADER ===");

            int i = 1;
            for (; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    break; // blank line separates headers and body
                sb.AppendLine(lines[i].Trim());
            }

            sb.AppendLine("\n=== ARTICLE BODY ===");

            for (i++; i < lines.Length; i++)
                sb.AppendLine(lines[i].Trim());

            return sb.ToString();
        }
        private string FormatXoverResponse(string response)
        {
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1)
                return "No overview data.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== ARTICLE OVERVIEW ===");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("224")) continue; // skip status line
                if (line.Trim() == ".") break;

                var parts = line.Split('\t');
                if (parts.Length >= 4)
                {
                    sb.AppendLine($"Article #{parts[0]}");
                    sb.AppendLine($"Subject: {parts[1]}");
                    sb.AppendLine($"From: {parts[2]}");
                    sb.AppendLine($"Date: {parts[3]}");
                    sb.AppendLine("------------------------------");
                }
                else
                {
                    sb.AppendLine(line); // fallback for malformed lines
                }
            }

            return sb.ToString();
        }


    }
}
// Enum to represent different command types for an initiel listview commandes
public enum CommandType
{
    LIST,
    GROUP,
    XOVER,
    ARTICLE,
    HEAD,
    POST,
    QUIT


}