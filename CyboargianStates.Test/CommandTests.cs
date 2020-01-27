using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
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
            var commandHandler = new CommandHandler();
            commandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            var result = commandHandler.Execute("ping");
            Assert.Equal(CommandStatus.Success, result.Status);
            Assert.Equal("Pong !", result.Content);
            
        }
    }
}
