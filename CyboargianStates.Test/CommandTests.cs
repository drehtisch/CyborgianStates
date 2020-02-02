using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CyboargianStates.Test
{
    public class CommandTests
    {
        [Fact]
        public void TestPingCommand()
        {
            CommandHandler.Clear();
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            var result = CommandHandler.Execute(new Message(0, "ping", new ConsoleMessageChannel(false)));
            Assert.Equal(CommandStatus.Success, result.Status);
            Assert.Equal("Pong !", result.Content);
        }
    }
}
