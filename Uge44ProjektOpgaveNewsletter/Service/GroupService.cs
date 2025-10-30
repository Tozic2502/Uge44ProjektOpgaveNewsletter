using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uge44ProjektOpgaveNewsletter.Service
{
    public class GroupService
    {
        private readonly DirectoryInfo _baseDirectory = new DirectoryInfo("UserGroups");

        public GroupService()
        {
            if (!_baseDirectory.Exists)
                _baseDirectory.Create();
        }

        public void SaveGroup(string userName, string groupName)
        {
            string userFile = Path.Combine(_baseDirectory.FullName, $"{userName}.txt");

            List<string> groups = new List<string>();
            if (File.Exists(userFile))
                groups.AddRange(File.ReadAllLines(userFile));

            if (!groups.Contains(groupName, StringComparer.OrdinalIgnoreCase))
                groups.Add(groupName);

            File.WriteAllLines(userFile, groups);
        }

        public List<string> LoadGroups(string userName)
        {
            string userFile = Path.Combine(_baseDirectory.FullName, $"{userName}.txt");
            if (!File.Exists(userFile))
                return new List<string>();

            return File.ReadAllLines(userFile).ToList();
        }
    }
}
