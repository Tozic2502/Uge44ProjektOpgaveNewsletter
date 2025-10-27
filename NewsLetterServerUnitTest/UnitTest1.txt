using System;
using Uge44ProjektOpgaveNewsletter.MainWindow.xaml;
using Uge44ProjektOpgaveNewsletter.MainWindow.xaml.cs;
using System.Collections.Generic;
using Xunit;



namespace NewsLetterServerUnitTest
{
    public class UnitTest1
    {
        [Fact]//User story 1
        public void connectionTest()
        {
            serverTexbox.Text = "news.sunsite.dk";//Server you are using
            portTextbox.text = "119";//Port for the server you are using
            connectionButton.PerformClick();

            bool isConnected = ConnectToServer(serverTexbox.Text, int.Parse(portTextbox.Text));
            if (isConnected)
            {
                Assert.Equal("200", _responseData);//insert server response code for successful connection
            }
            if (_responseData == "200")
            {
                userNameTextBox.Text = "mikras01@easv365.dk";//UserName for the server you are using
                passworBox.Text = "12eb1b";//Password for the server you are using
                confirmButton.PerformClick();

                Assert.Equal("281", _responseData);
            }

        }
        [Fact]//User story 2
        public void getListOfgroupsTest()
        {
            commandoTextbox.text = "LIST";
            confirmCommandoButton.PerformClick();

            Assert.Equal("215", _responseData);//insert server response code for successful group list retrieval

        }
        [Fact]//User story 3
        public void selectGroupTest()
        {
            commandoTextbox.text = "GROUP group";//insert group name
            confirmCommandoButton.PerformClick();

            Assert.Equal("211", _responseData);//insert server response code for successful group selection
        }
        [Fact]//User story 4
        public void selectHeadlineTest()
        {
            commandoTextbox.text = "Xover group/articleNumber";//insert group name and article number'
            confirmCommandoButton.PerformClick();

            Assert.Equal("224", _responseData);//insert server response code for successful headline retrieval

        }
        [Fact]//User story 5
        public void postAuthTest()
        {
            commandoTextbox.text = "POST";//gets reply code from server to see if user is authorized to post

            Assert.Equal("340", _responseData);//insert server response code for authorized to post
        }
        [Fact]//User story 6
        public void postArticleTest()
        {
            commandoTextbox.text = "POST";//insert article to post

            Assert.Equal("240", _responseData);//insert server response code for successful post
        }
        [Fact]//User story 7
        public void saveGroupTest()
        {
            // Arrange
            var service = new GroupService();
            string commandTextboxInput = "Developers";

            // Act
            service.SaveGroup(commandTextboxInput);
            List<string> savedGroups = service.GetGroups();

            // Assert
            Assert.Equal(1, savedGroups.Count, "Group list should contain one item.");
            Assert.Equal("Developers", savedGroups[0], "Saved group name should match input.");

        }
        [Fact]//user story 8
        public void loadSavedGroupTest()
        {
            // Arrange
            var service = new GroupService();
            service.SaveGroup("Developers");
            // Act
            List<string> loadedGroups = service.LoadGroups();
            // Assert
            Assert.Equal(1, loadedGroups.Count, "Loaded group list should contain one item.");
            Assert.Equal("Developers", loadedGroups[0], "Loaded group name should match saved group.");

        }
        [Fact]
        public void disconnectTest()
        {
            commandoTextbox.text = "QUIT";//Command to disconnect from server

            Assert.Equal("205", _responseData);//insert server response code for successful disconnection

        }


    }
}
