using CyborgianStates.MessageHandling;
using Discord.WebSocket;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.MessageHandling
{
    public class DiscordMessageHandlerTests
    {

        private Mock<IOptions<AppSettings>> appSettingsMock;
        private Mock<DiscordClientWrapper> clientMock;
        public DiscordMessageHandlerTests()
        {
            appSettingsMock = new Mock<IOptions<AppSettings>>(MockBehavior.Strict);
            appSettingsMock
                .Setup(m => m.Value)
                .Returns(new AppSettings() { SeperatorChar = '$', DiscordBotLoginToken = string.Empty });
            var mockClient = new Mock<DiscordClientWrapper>(MockBehavior.Strict);
            mockClient.Setup(m => m.LoginAsync(It.IsAny<Discord.TokenType>(), "", true))
                .Returns(Task.CompletedTask);
            mockClient.Setup(m => m.StartAsync()).Returns(Task.CompletedTask);
            mockClient.Setup(m => m.LogoutAsync()).Returns(Task.CompletedTask);
            mockClient.Setup(m => m.StopAsync()).Returns(Task.CompletedTask);
            mockClient.Setup(m => m.Dispose());
            mockClient.SetupGet<bool>(m => m.IsTest).Returns(true);
            clientMock = mockClient;
            
        }
        [Fact]
        public async Task TestSimpleEventLogMethods()
        {
            var handler = new DiscordMessageHandler(appSettingsMock.Object, clientMock.Object);
            await handler.InitAsync();
            Assert.Throws<ArgumentNullException>(() => { new DiscordMessageHandler(null, clientMock.Object); });
            Assert.Throws<ArgumentNullException>(() => { new DiscordMessageHandler(appSettingsMock.Object, null); });
            clientMock.Raise(m => m.Ready += null);
            handler.IsRunning.Should().BeTrue();
            clientMock.Raise(m => m.Connected += null);
            clientMock.Raise(m => m.LoggedIn += null);
            clientMock.Raise(m => m.Disconnected += null, new Exception());
            clientMock.Raise(m => m.LoggedOut += null);
        }
    }
}
