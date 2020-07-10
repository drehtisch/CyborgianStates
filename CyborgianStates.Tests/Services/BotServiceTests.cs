using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
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
        public BotServiceTests()
        {
            msgHandlerMock = new Mock<IMessageHandler>(MockBehavior.Strict);
            msgHandlerMock.Setup(m => m.InitAsync()).Returns(Task.CompletedTask);
            msgHandlerMock.Setup(m => m.RunAsync()).Returns(Task.CompletedTask);
            msgHandlerMock.Setup(m => m.ShutdownAsync()).Returns(Task.CompletedTask);
            ConfigureServicesForTests();
        }
        [Fact]
        public async Task TestInitRunAndShutDownBotService()
        {
            var msgHandler = msgHandlerMock.Object;
            var botService = new BotService(msgHandler);
            await botService.InitAsync().ConfigureAwait(false);
            await botService.RunAsync().ConfigureAwait(false);
            Assert.True(botService.IsRunning);
            await botService.ShutdownAsync().ConfigureAwait(false);
            Assert.False(botService.IsRunning);
        }

        private static void ConfigureServicesForTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<HttpDataService, HttpDataService>();
            serviceCollection.AddSingleton<IRequestDispatcher, RequestDispatcher>();
            Program.ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task TestStartupProgressMessageAndShutDown()
        {
            CommandHandler.Clear();
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            Assert.True(CommandHandler.Count == 1);
            ConfigureServicesForTests();
            BotService botService = new BotService(msgHandlerMock.Object);
            await botService.InitAsync().ConfigureAwait(false);
            await botService.RunAsync().ConfigureAwait(false);
            Assert.True(botService.IsRunning);
            msgHandlerMock.Verify(m => m.InitAsync(), Times.Once);
            msgHandlerMock.Verify(m => m.RunAsync(), Times.Once);
            bool isPublic = false;
            CommandResponse commandResponse = new CommandResponse(CommandStatus.Error, "");

            Mock<IMessageChannel> msgChannelMock = new Mock<IMessageChannel>(MockBehavior.Strict);
            msgChannelMock.SetupGet(m => m.IsPrivate).Returns(true);

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

            await Task.Delay(1000).ConfigureAwait(false);

            Assert.True(isPublic);
            Assert.Equal(CommandStatus.Success, commandResponse.Status);
            Assert.Equal("Pong !", commandResponse.Content);

            await botService.ShutdownAsync().ConfigureAwait(false);
            Assert.False(botService.IsRunning);
            msgHandlerMock.Verify(m => m.ShutdownAsync(), Times.Once);
        }
        [Fact]
        public void TestBotServiceWithNullMessageHandler()
        {
            Assert.Throws<ArgumentNullException>(() => new BotService(null));
        }
    }
}
