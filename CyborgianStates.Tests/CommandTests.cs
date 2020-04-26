using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests
{
    public class CommandTests
    {
        [Fact]
        public async Task TestPingCommand()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await new PingCommand().Execute(null).ConfigureAwait(false)).ConfigureAwait(false);
            var result = await new PingCommand().Execute(new Message(0, "ping", new ConsoleMessageChannel(false))).ConfigureAwait(false);
            Assert.Equal(CommandStatus.Success, result.Status);
            Assert.Equal("Pong !", result.Content);
        }
    }
}
