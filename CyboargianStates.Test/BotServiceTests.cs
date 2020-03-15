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

namespace CyborgianStates.Test
{
    public class BotServiceTests
    {
        Mock<IMessageHandler> msgHandlerMock;
        public BotServiceTests()
        {
            msgHandlerMock = new Mock<IMessageHandler>();
            ServiceCollection serviceCollection = ConfigureServicesForTests();
            Program.ServiceProvider = serviceCollection.BuildServiceProvider();
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

        private static ServiceCollection ConfigureServicesForTests()
        {
            var serviceCollection = new ServiceCollection();
            var httpServiceMock = new Mock<IHttpDataService>();
            serviceCollection.AddSingleton(typeof(IHttpDataService), httpServiceMock.Object);
            serviceCollection.AddSingleton<IRequestDispatcher, RequestDispatcher>();
            return serviceCollection;
        }

        [Fact]
        public async Task TestStartupProgressMessageAndShutDown()
        {
            CommandHandler.Clear();
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            Assert.True(CommandHandler.Count == 1);

            msgHandlerMock.Setup(m => m.InitAsync());
            msgHandlerMock.Setup(m => m.RunAsync());
            msgHandlerMock.Setup(m => m.ShutdownAsync());
            BotService botService = new BotService(msgHandlerMock.Object);
            await botService.InitAsync().ConfigureAwait(false);
            await botService.RunAsync().ConfigureAwait(false);
            Assert.True(botService.IsRunning);
            msgHandlerMock.Verify(m => m.InitAsync(), Times.Once);
            msgHandlerMock.Verify(m => m.RunAsync(), Times.Once);
            bool isPublic = false;
            CommandResponse commandResponse = new CommandResponse(CommandStatus.Error, "");

            Mock<IMessageChannel> msgChannelMock = new Mock<IMessageChannel>();
            msgChannelMock.Setup(m => m.WriteToAsync(It.IsAny<bool>(), It.IsAny<CommandResponse>()))
                .Callback<bool, CommandResponse>((b, cr) =>
                {
                    isPublic = b;
                    commandResponse = cr;
                });

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
