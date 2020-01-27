using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Interfaces;
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
            var commandHandler = new CommandHandler();
            commandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            Assert.True(commandHandler.Count == 1, $"Expected CommandCount to be 1 but was: {commandHandler.Count}");
        }
        [Fact]
        public void TestRegisterCommandWithInvalidCommandDefinition()
        {
            var commandHandler = new CommandHandler();
            Assert.Throws<ArgumentNullException>(() => commandHandler.Register(null));
            Assert.Throws<InvalidOperationException>(() => commandHandler.Register(new CommandDefinition(typeof(ICommand), new List<string>())));
            Assert.Throws<InvalidOperationException>(() => commandHandler.Register(new CommandDefinition(null, new List<string>() { "ping" })));
            Assert.Throws<InvalidOperationException>(() => commandHandler.Register(new CommandDefinition(typeof(string), new List<string>() { "ping" })));
        }
        [Fact]
        public void TestResolvePingCommand()
        {
            var commandHandler = new CommandHandler();
            commandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            var resolved = commandHandler.Resolve("ping");
            Assert.True(resolved is PingCommand);
        }
        [Fact]
        public void TestResolveWithEmptyTrigger()
        {
            var commandHandler = new CommandHandler();
            Assert.Throws<ArgumentNullException>(() => commandHandler.Resolve(null));
            Assert.Throws<ArgumentNullException>(() => commandHandler.Resolve(string.Empty));
            Assert.Throws<ArgumentNullException>(() => commandHandler.Resolve("    "));
        }
        [Fact]
        public void TestResolveUnknownCommand()
        {
            var commandHandler = new CommandHandler();
            var result = commandHandler.Resolve("unknownCommand");
            Assert.Null(result);
        }
        [Fact]
        public void TestExecutePingCommand()
        {
            var commandHandler = new CommandHandler();
            commandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            var result = commandHandler.Execute("ping");
            Assert.True(result is CommandResponse);
        }
    }
}
