using System.Collections.Generic;
using System.Threading.Tasks;
using Uge44ProjektOpgaveNewsletter.Service;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        private readonly ConnectionService _connection = ConnectionService.Instance;

        [Fact] // User story 1
        public async Task ConnectionTest()
        {
            string response = await _connection.ConnectAsync("news.sunsite.dk", 119);
            Assert.StartsWith("200", response);
        }

        [Fact] // User story 2
        public async Task GetListOfGroupsTest()
        {
            string username = "mikras01@easv365.dk";
            string password = "12eb1b";
            await _connection.ConnectAsync("news.sunsite.dk", 119);
            await _connection.SendCommandAsync($"AUTHINFO USER {username}");
            await _connection.SendCommandAsync($"AUTHINFO PASS {password}");
            string response = await _connection.SendCommandAsync("LIST");
            Assert.StartsWith("215", response);
        }

        [Fact] // User story 3
        public async Task SelectGroupTest()
        {
            string username = "mikras01@easv365.dk";
            string password = "12eb1b";
            await _connection.ConnectAsync("news.sunsite.dk", 119);
            await _connection.SendCommandAsync($"AUTHINFO USER {username}");
            await _connection.SendCommandAsync($"AUTHINFO PASS {password}");

            string response = await _connection.SendCommandAsync("GROUP dk.test");
            Assert.StartsWith("211", response);
        }

        [Fact] // User story 4
        public async Task SelectArticleTest()
        {
            string username = "mikras01@easv365.dk";
            string password = "12eb1b";
            await _connection.ConnectAsync("news.sunsite.dk", 119);
            await _connection.SendCommandAsync($"AUTHINFO USER {username}");    
            await _connection.SendCommandAsync($"AUTHINFO PASS {password}");
            await _connection.SendCommandAsync("GROUP dk.test");
            string response = await _connection.SendCommandAsync("XOVER 1-10");
            Assert.StartsWith("224", response);
        }

        [Fact] // User story 5
        public async Task PostAuthTest()
        {
            string username = "mikras01@easv365.dk";
            string password = "12eb1b";

            await _connection.ConnectAsync("news.sunsite.dk", 119);
            await _connection.SendCommandAsync($"AUTHINFO USER {username}");
            await _connection.SendCommandAsync($"AUTHINFO PASS {password}");
            string response = await _connection.SendCommandAsync("POST");

            Assert.StartsWith("340", response);
        }

        [Fact] // User story 7
        public void SaveGroupTest()
        {
            var service = new GroupService();
            string userName = "Developers";
            string groupName = "SomeGroup";

            service.SaveGroup(userName, groupName);
            List<string> savedGroups = service.LoadGroups(userName);

            Assert.Contains(groupName, savedGroups);
        }

        [Fact] // User story 8
        public void LoadSavedGroupTest()
        {
            var service = new GroupService();
            string userName = "Developers";
            string groupName = "SomeGroup";

            service.SaveGroup(userName, groupName);
            List<string> loadedGroups = service.LoadGroups(userName);

            Assert.Contains(groupName, loadedGroups);
        }

        [Fact]
        public async Task DisconnectTest()
        {
            await _connection.ConnectAsync("news.sunsite.dk", 119);

            string response = await _connection.SendCommandAsync("QUIT");
            Assert.StartsWith("205", response);
        }
    }
}
