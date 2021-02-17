using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.CommandTests
{
    public class AboutCommandTests
    {
        [Fact]
        public async Task TestExecuteSuccess()
        {
            ConfigureServices();
            var message = new Message(0, "about", new ConsoleMessageChannel());
            var command = new AboutCommand();
            var response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Success);
            response.Content.Should().StartWith($"About CyborgianStates{Environment.NewLine}{Environment.NewLine}Contact for this instance{Environment.NewLine}contact@example.com{Environment.NewLine}{Environment.NewLine}Github{Environment.NewLine}[CyborgianStates](https://github.com/Free-Nations-Region/CyborgianStates){Environment.NewLine}{Environment.NewLine}Developed by Drehtisch{Environment.NewLine}Discord: Drehtisch#5680{Environment.NewLine}NationStates: [Tigerania](https://www.nationstates.net/nation=tigerania){Environment.NewLine}{Environment.NewLine}Support{Environment.NewLine}via [OpenCollective](https://opencollective.com/fnr){Environment.NewLine}");
        }
        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            var options = new Mock<IOptions<AppSettings>>(MockBehavior.Strict);
            options.SetupGet(m => m.Value).Returns(new AppSettings() { SeperatorChar = '$' , Contact = "contact@example.com" });
            serviceCollection.AddSingleton<IResponseBuilder, ConsoleResponseBuilder>();
            serviceCollection.AddSingleton(typeof(IOptions<AppSettings>), options.Object);
            Program.ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
