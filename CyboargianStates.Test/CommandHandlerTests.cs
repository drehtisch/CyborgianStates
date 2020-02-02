using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CyboargianStates.Test
{
    public class CommandHandlerTests
    {
        [Fact]
        public void TestRegisterCommand()
        {
            CommandHandler.Clear();
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            Assert.True(CommandHandler.Count == 1, $"Expected CommandCount to be 1 but was: {CommandHandler.Count}");
        }
        [Fact]
        public void TestRegisterCommandWithInvalidCommandDefinition()
        {
            Assert.Throws<ArgumentNullException>(() => CommandHandler.Register(null));
            Assert.Throws<InvalidOperationException>(() => CommandHandler.Register(new CommandDefinition(typeof(ICommand), new List<string>())));
            Assert.Throws<InvalidOperationException>(() => CommandHandler.Register(new CommandDefinition(null, new List<string>() { "ping" })));
            Assert.Throws<InvalidOperationException>(() => CommandHandler.Register(new CommandDefinition(typeof(string), new List<string>() { "ping" })));
        }
        [Fact]
        public void TestResolvePingCommand()
        {
            CommandHandler.Clear();
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            var resolved = CommandHandler.Resolve("ping");
            Assert.True(resolved is PingCommand);
        }
        [Fact]
        public void TestResolveWithEmptyTrigger()
        {
            Assert.Throws<ArgumentNullException>(() => CommandHandler.Resolve(null));
            Assert.Throws<ArgumentNullException>(() => CommandHandler.Resolve(string.Empty));
            Assert.Throws<ArgumentNullException>(() => CommandHandler.Resolve("    "));
        }
        [Fact]
        public void TestResolveUnknownCommand()
        {
            var result = CommandHandler.Resolve("unknownCommand");
            Assert.Null(result);
        }
        [Fact]
        public void TestExecutePingCommand()
        {
            CommandHandler.Clear();
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            var result = CommandHandler.Execute(new Message(0, "ping", new ConsoleMessageChannel(false)));
            Assert.True(result is CommandResponse);
            Assert.Equal(CommandStatus.Success, result.Status);
        }
        [Fact]
        public void TestExecuteWithEmptyMessage()
        {
            Assert.Throws<ArgumentNullException>(() => CommandHandler.Execute(null));
        }
    }
}
