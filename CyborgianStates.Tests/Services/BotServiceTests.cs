using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using CyborgianStates.Models;
using CyborgianStates.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.Services
{

    public class BotServiceTests
    {
        Mock<IMessageHandler> msgHandlerMock;
        Mock<IRequestDispatcher> requestDispatcherMock;
        Mock<IUserRepository> userRepositoryMock;
        Mock<IMessageChannel> msgChannelMock;
        public BotServiceTests()
        {
            msgHandlerMock = new Mock<IMessageHandler>(MockBehavior.Strict);
            msgHandlerMock.Setup(m => m.InitAsync()).Returns(Task.CompletedTask);
            msgHandlerMock.Setup(m => m.RunAsync()).Returns(Task.CompletedTask);
            msgHandlerMock.Setup(m => m.ShutdownAsync()).Returns(Task.CompletedTask);

            requestDispatcherMock = new Mock<IRequestDispatcher>(MockBehavior.Strict);
            requestDispatcherMock.Setup(r => r.Register(It.IsAny<DataSourceType>(), It.IsAny<IRequestQueue>())).Returns(() => Task.CompletedTask);

            userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);

            msgChannelMock = new Mock<IMessageChannel>(MockBehavior.Strict);
            msgChannelMock.SetupGet(m => m.IsPrivate).Returns(true);
        }
        [Fact]
        public async Task TestInitRunAndShutDownBotService()
        {
            Program.ServiceProvider = Program.ConfigureServices();
            var botService = new BotService(msgHandlerMock.Object, requestDispatcherMock.Object, userRepositoryMock.Object);
            await botService.InitAsync().ConfigureAwait(false);
            await botService.RunAsync().ConfigureAwait(false);
            Assert.True(botService.IsRunning);
            await botService.ShutdownAsync().ConfigureAwait(false);
            Assert.False(botService.IsRunning);
        }

        [Fact]
        public async Task TestStartupProgressMessageAndShutDown()
        {
            userRepositoryMock.Setup(u => u.IsUserInDbAsync(It.IsAny<ulong>())).Returns(() => Task.FromResult(true));
            userRepositoryMock.Setup(u => u.AddUserToDbAsync(It.IsAny<ulong>())).Returns(() => Task.CompletedTask);
            userRepositoryMock.Setup(u => u.IsAllowedAsync(It.IsAny<string>(), It.IsAny<ulong>())).Returns(() => Task.FromResult(true));

            CommandHandler.Clear();
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));

            Assert.True(CommandHandler.Count == 1);

            var botService = new BotService(msgHandlerMock.Object, requestDispatcherMock.Object, userRepositoryMock.Object);
            await botService.InitAsync().ConfigureAwait(false);
            await botService.RunAsync().ConfigureAwait(false);

            Assert.True(botService.IsRunning);
            msgHandlerMock.Verify(m => m.InitAsync(), Times.Once);
            msgHandlerMock.Verify(m => m.RunAsync(), Times.Once);

            bool isPublic = false;
            CommandResponse commandResponse = new CommandResponse(CommandStatus.Error, "");

            msgChannelMock.Setup(m => m.WriteToAsync(It.IsAny<bool>(), It.IsAny<CommandResponse>()))
                .Callback<bool, CommandResponse>((b, cr) =>
                {
                    isPublic = b;
                    commandResponse = cr;
                })
                .Returns(Task.CompletedTask);

            Message message = new Message(0, "ping", msgChannelMock.Object);
            MessageReceivedEventArgs eventArgs = new MessageReceivedEventArgs(message);
            msgHandlerMock.Raise(m => m.MessageReceived += null, this, eventArgs);

            message = new Message(0, "test", msgChannelMock.Object);
            eventArgs = new MessageReceivedEventArgs(message);
            msgHandlerMock.Raise(m => m.MessageReceived += null, this, eventArgs);

            message = new Message(1, "test", msgChannelMock.Object);
            eventArgs = new MessageReceivedEventArgs(message);
            msgHandlerMock.Raise(m => m.MessageReceived += null, this, eventArgs);

            await Task.Delay(1000).ConfigureAwait(false);

            Assert.True(isPublic);
            Assert.Equal(CommandStatus.Success, commandResponse.Status);
            Assert.Equal("Pong !", commandResponse.Content);

            await botService.ShutdownAsync().ConfigureAwait(false);

            Assert.False(botService.IsRunning);
            msgHandlerMock.Verify(m => m.ShutdownAsync(), Times.Once);
        }

        [Fact]
        public async Task TestIsRelevant()
        {
            Program.ServiceProvider = Program.ConfigureServices();
            userRepositoryMock.Setup(u => u.IsUserInDbAsync(It.IsAny<ulong>())).Returns(() => Task.FromResult(false));
            userRepositoryMock.Setup(u => u.AddUserToDbAsync(It.IsAny<ulong>())).Returns(() => Task.CompletedTask);
            userRepositoryMock.Setup(u => u.IsAllowedAsync(It.IsAny<string>(), It.IsAny<ulong>())).Returns(() => Task.FromResult(true));
            msgChannelMock.Setup(m => m.WriteToAsync(It.IsAny<bool>(), It.IsAny<CommandResponse>())).Returns(Task.CompletedTask);

            Message message = new Message(0, "test", msgChannelMock.Object);
            var botService = new BotService(msgHandlerMock.Object, requestDispatcherMock.Object, userRepositoryMock.Object);
            await botService.InitAsync().ConfigureAwait(false);
            await botService.RunAsync().ConfigureAwait(false);

            MessageReceivedEventArgs eventArgs = new MessageReceivedEventArgs(null);
            msgHandlerMock.Raise(m => m.MessageReceived += null, this, eventArgs);

            eventArgs = new MessageReceivedEventArgs(message);
            msgHandlerMock.Raise(m => m.MessageReceived += null, this, eventArgs);

            AppSettings.IsTesting = true;
            AppSettings.Configuration = "test";
            eventArgs = new MessageReceivedEventArgs(message);
            msgHandlerMock.Raise(m => m.MessageReceived += null, this, eventArgs);
            AppSettings.IsTesting = false;

            message = new Message(1, "test", msgChannelMock.Object);            
            eventArgs = new MessageReceivedEventArgs(message);
            msgHandlerMock.Raise(m => m.MessageReceived += null, this, eventArgs);
        }


        [Fact]
        public void TestBotServiceWithNullMessageHandler()
        {
            Assert.Throws<ArgumentNullException>(() => new BotService(null, requestDispatcherMock.Object, userRepositoryMock.Object));
            Assert.Throws<ArgumentNullException>(() => new BotService(msgHandlerMock.Object, null, userRepositoryMock.Object));
            Assert.Throws<ArgumentNullException>(() => new BotService(msgHandlerMock.Object, requestDispatcherMock.Object, null));
        }
    }
}
