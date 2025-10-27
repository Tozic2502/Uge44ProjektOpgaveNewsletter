using System;
using System.Collections.Generic;
using System.IO;

public class SaveUserService
{
    private Dictionary<string, string> users = new Dictionary<string, string>();

    public void LoadUsers(string filePath)
    {
        users.Clear();
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split(':');
            if (parts.Length == 2)
            {
                var username = parts[0].Trim();
                var password = parts[1].Trim();
                if (!users.ContainsKey(username))
                    users.Add(username, password);
            }
        }
    }

    public List<string> GetUsernames()
    {
        return new List<string>(users.Keys);
    }

    public string GetPassword(string username)
    {
        return users.ContainsKey(username) ? users[username] : string.Empty;
    }
    public void SaveUser(string username, string password, string filePath)
    {
        // Ensure the directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Reload existing users first
        LoadUsers(filePath);

        // If user already exists, update password
        users[username] = password;

        // Rewrite file with all users
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            foreach (var kvp in users)
            {
                writer.WriteLine($"{kvp.Key}:{kvp.Value}");
            }
        }
    }
}