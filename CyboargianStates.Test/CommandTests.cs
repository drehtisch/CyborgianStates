using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CyboargianStates.Test
{
    public class CommandTests
    {
        [Fact]
        public async Task TestPingCommand()
        {
            CommandHandler.Clear();
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            var result = await CommandHandler.Execute(new Message(0, "ping", new ConsoleMessageChannel(false))).ConfigureAwait(false);
            Assert.Equal(CommandStatus.Success, result.Status);
            Assert.Equal("Pong !", result.Content);
        }
    }
}
