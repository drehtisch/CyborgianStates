using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Exceptions;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using CyborgianStates.Tests.CommandHandling;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace CyborgianStates.Tests
{
    public class NationStatsCommandTests
    {
        [Fact]
        public async Task TestEmptyExecute()
        {
            ConfigureServices();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await new NationStatsCommand().Execute(null).ConfigureAwait(false)).ConfigureAwait(false);
            var message = new Message(0, "nation", new ConsoleMessageChannel());
            var response = await new NationStatsCommand().Execute(message);
            response.Status.Should().Be(CommandStatus.Error);
            response.Content.Should().StartWith("Something went wrong\r\n\r\nNo parameter passed.");
        }
        [Fact]
        public async Task TestCancel()
        {
            ConfigureServices();
            var message = new Message(0, "nation Testlandia", new ConsoleMessageChannel());
            var command = new NationStatsCommand();
            var source = new CancellationTokenSource();
            source.CancelAfter(250);
            command.SetCancellationToken(source.Token);
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestStatus.Canceled);
            var response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Error);
            response.Content.Should().StartWith("Something went wrong\r\n\r\nRequest/Command has been canceled. Sorry :(");
        }

        [Fact]
        public async Task TestExecuteWithErrors()
        {
            ConfigureServices();
            var message = new Message(0, "nation Testlandia", new ConsoleMessageChannel());
            var command = new NationStatsCommand();
            // HttpRequestFailed
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestStatus.Failed, exception: new HttpRequestFailedException(), failReason: "Request failed: 404 - NotFound");
            var response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Error);
            response.Content.Should().StartWith("Something went wrong\r\n\r\nRequest failed: 404 - NotFound");
            // InvalidOperation
            response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Error);
            response.Content.Should().StartWith("Something went wrong\r\n\r\nAn unexpected error occured. Please contact the bot administrator.");
        }

        [Fact]
        public async Task TestExecuteSuccess()
        {
            ConfigureServices();
            var message = new Message(0, "nation Testlandia", new ConsoleMessageChannel());
            var command = new NationStatsCommand();
            var xml = File.ReadAllText(Path.Combine("Data", "testlandia-nation-stats.xml"));
            var xmlResult = new XmlDocument();
            xmlResult.LoadXml(xml);
            TestRequestDispatcher.PrepareNextRequest(response: xmlResult);
            var response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Success);
            response.Content.Should().StartWith("BasicStats for Nation\r\n\r\n**[The дСвобода Мысл of Testlandia](https://www.nationstates.net/nation=testlandia)**\r\n38.041 billion Testlandians | Founded 0 | Last active 5 days ago\r\n\r\nRegion                                                            Residency \r\n[Testregionia](https://www.nationstates.net/region=testregionia)   95 days  \r\n\r\nNew York Times Democracy\r\nC: Excellent (68.90) | E: Very Strong (80.00) | P: Superb (76.29)\r\n\r\nWA Member\r\nGA Vote: UNDECIDED | SC Vote: UNDECIDED |  0.00 endorsements | 6102.00 Influence (Eminence Grise)");
        }

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            var options = new Mock<IOptions<AppSettings>>(MockBehavior.Strict);
            options.SetupGet(m => m.Value).Returns(new AppSettings() { SeperatorChar = '$' });
            serviceCollection.AddSingleton<IRequestDispatcher, TestRequestDispatcher>();
            serviceCollection.AddSingleton<IResponseBuilder, ConsoleResponseBuilder>();
            serviceCollection.AddSingleton(typeof(IOptions<AppSettings>), options.Object);
            Program.ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}