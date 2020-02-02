using CyborgianStates;
using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CyboargianStates.Test
{
    public class BotServiceTests
    {
        [Fact]
        public async Task TestInitRunAndShutDownBotService()
        {
            var msgHandlerMock = new Mock<IMessageHandler>();
            var msgHandler = msgHandlerMock.Object;
            var botService = new BotService(msgHandler);
            await botService.InitAsync().ConfigureAwait(false);
            await botService.RunAsync().ConfigureAwait(false);
            Assert.True(botService.IsRunning);
            await botService.ShutdownAsync().ConfigureAwait(false);
            Assert.False(botService.IsRunning);
        }
        [Fact]
        public async Task TestStartupProgressMessageAndShutDown()
        {
            CommandHandler.Clear();
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            Assert.True(CommandHandler.Count == 1);

            Mock<IMessageHandler> msgHandlerMock = new Mock<IMessageHandler>();
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
            
            await Task.Delay(1000).ConfigureAwait(false);

            Assert.True(isPublic);
            Assert.Equal(CommandStatus.Success, commandResponse.Status);
            Assert.Equal("Pong !", commandResponse.Content);

            await botService.ShutdownAsync().ConfigureAwait(false);
            Assert.False(botService.IsRunning);
            msgHandlerMock.Verify(m => m.ShutdownAsync(), Times.Once);
        }
    }
}
