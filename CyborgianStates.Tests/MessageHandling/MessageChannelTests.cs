using CyborgianStates.CommandHandling;
using CyborgianStates.MessageHandling;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.MessageHandling
{
    public class MessageChannelTests
    {
        [Fact]
        public async Task MessageChannelTest()
        {
            var channel = new ConsoleMessageChannel(false);
            await channel.WriteToAsync(true, new CommandResponse(CommandStatus.Success, "Test")).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await channel.WriteToAsync(true, null).ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}