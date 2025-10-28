using System;
using System.Collections.Generic;
using System.IO;

public class SaveUserService
{
    private class UserInfo
    {
        public string Password { get; set; } = "";
        public string ServerName { get; set; } = "";
        public string Port { get; set; } = "";
    }

    private Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();

    public void LoadUsers(string filePath)
    {
        users.Clear();
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            // Format: username:password:servername:port
            var parts = line.Split(':');
            if (parts.Length >= 2)
            {
                var username = parts[0].Trim();
                var password = parts[1].Trim();
                var serverName = parts.Length >= 3 ? parts[2].Trim() : "";
                var port = parts.Length >= 4 ? parts[3].Trim() : "";

                if (!users.ContainsKey(username))
                {
                    users.Add(username, new UserInfo
                    {
                        Password = password,
                        ServerName = serverName,
                        Port = port
                    });
                }
            }
        }
    }

    public List<string> GetUsernames()
    {
        return new List<string>(users.Keys);
    }

    public string GetPassword(string username)
    {
        return users.ContainsKey(username) ? users[username].Password : string.Empty;
    }

    public string GetServerName(string username)
    {
        return users.ContainsKey(username) ? users[username].ServerName : string.Empty;
    }

    public string GetPort(string username)
    {
        return users.ContainsKey(username) ? users[username].Port : string.Empty;
    }

    public void SaveUser(string username, string password, string serverName, string port, string filePath)
    {
        // Ensure the directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Reload existing users first
        LoadUsers(filePath);

        // Add or update user info
        users[username] = new UserInfo
        {
            Password = password,
            ServerName = serverName,
            Port = port
        };

        // Rewrite file with all users
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            foreach (var kvp in users)
            {
                writer.WriteLine($"{kvp.Key}:{kvp.Value.Password}:{kvp.Value.ServerName}:{kvp.Value.Port}");
            }
        }
    }
}
