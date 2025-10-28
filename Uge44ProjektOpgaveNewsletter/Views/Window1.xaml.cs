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
    public partial class Window1 : Window
    {
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private TcpClient? _client;
        private string? _responseData;

        public Window1()
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

            InfoTextBlock.Text = _responseData;
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
                _writer.WriteLine(commando);
                _writer.Flush();

                StringBuilder responseData = new StringBuilder();
                string line;

                while ((line = _reader.ReadLine()) != null)
                {
                    // Check for server error codes (e.g., 4xx, 5xx)
                    if (line.StartsWith("4") || line.StartsWith("5"))
                    {
                        responseData.AppendLine($"Server error: {line}");
                        break;
                    }

                    if (line == "." || string.IsNullOrWhiteSpace(line))
                        break;

                    responseData.AppendLine(line);
                }

                _responseData = responseData.ToString();
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