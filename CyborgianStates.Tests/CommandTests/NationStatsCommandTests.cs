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
            response.Content.Should().StartWith($"Something went wrong{Environment.NewLine}{Environment.NewLine}No parameter passed.");
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
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestType.GetBasicNationStats, Enums.RequestStatus.Canceled);
            var response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Error);
            response.Content.Should().StartWith($"Something went wrong{Environment.NewLine}{Environment.NewLine}Request/Command has been canceled. Sorry :(");
        }

        [Fact]
        public async Task TestExecuteWithErrors()
        {
            ConfigureServices();
            var message = new Message(0, "nation Testlandia", new ConsoleMessageChannel());
            var command = new NationStatsCommand();
            // HttpRequestFailed
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestType.GetBasicNationStats, Enums.RequestStatus.Failed, exception: new HttpRequestFailedException(), failReason: "Request failed: 404 - NotFound");
            var response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Error);
            response.Content.Should().StartWith($"Something went wrong{Environment.NewLine}{Environment.NewLine}Request failed: 404 - NotFound");
            // InvalidOperation
            response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Error);
            response.Content.Should().StartWith($"Something went wrong{Environment.NewLine}{Environment.NewLine}An unexpected error occured. Please contact the bot administrator.");
            // NationStats Response not XmlDocument
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestType.GetBasicNationStats);
            response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Error);
            response.Content.Should().StartWith($"Something went wrong{Environment.NewLine}{Environment.NewLine}An unexpected error occured. Please contact the bot administrator.");
            // RegionalOfficer Response not XmlDocument
            var nstatsXml = File.ReadAllText(Path.Combine("Data", "testlandia-nation-stats.xml"));
            var nstatsXmlResult = new XmlDocument();
            nstatsXmlResult.LoadXml(nstatsXml);
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestType.GetBasicNationStats, response: nstatsXmlResult);
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestType.GetRegionalOfficers);
            response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Error);
            response.Content.Should().StartWith($"Something went wrong{Environment.NewLine}{Environment.NewLine}An unexpected error occured. Please contact the bot administrator.");
            // Empty RegionalOfficer Xml
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestType.GetBasicNationStats, response: nstatsXmlResult);
            var roResult = new XmlDocument();
            roResult.LoadXml("<xml></xml>");
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestType.GetRegionalOfficers, response: roResult);
            response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Success);
            response.Content.Should().StartWith($"The дСвобода Мысл of Testlandia");
        }

        [Fact]
        public async Task TestExecuteSuccess()
        {
            ConfigureServices();
            var message = new Message(0, "nation Testlandia", new ConsoleMessageChannel());
            var command = new NationStatsCommand();
            var nstatsXml = File.ReadAllText(Path.Combine("Data", "testlandia-nation-stats.xml"));
            var nstatsXmlResult = new XmlDocument();
            nstatsXmlResult.LoadXml(nstatsXml);
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestType.GetBasicNationStats, response: nstatsXmlResult);
            var rofficersXml = File.ReadAllText(Path.Combine("Data", "testregionia-officers.xml"));
            var rofficersXmlResult = new XmlDocument();
            rofficersXmlResult.LoadXml(rofficersXml);
            TestRequestDispatcher.PrepareNextRequest(Enums.RequestType.GetRegionalOfficers, response: rofficersXmlResult);
            var response = await command.Execute(message);
            response.Status.Should().Be(CommandStatus.Success);
            response.Content.Should().StartWith($"The дСвобода Мысл of Testlandia{Environment.NewLine}{Environment.NewLine}38.068 billion Testlandians | Last active 7 hours ago{Environment.NewLine}{Environment.NewLine}Founded{Environment.NewLine}01.01.1970 (0){Environment.NewLine}{Environment.NewLine}Region                                                            Regional Officer                   {Environment.NewLine}[Testregionia](https://www.nationstates.net/region=testregionia)  &lt;h1&gt;Field Tester&lt;/h1&gt;  {Environment.NewLine}{Environment.NewLine}Resident Since{Environment.NewLine}12.11.2020 (99 d){Environment.NewLine}{Environment.NewLine}New York Times Democracy{Environment.NewLine}C: Excellent (69.19) | E: Very Strong (80.00) | P: Superb (76.29){Environment.NewLine}{Environment.NewLine}WA Member{Environment.NewLine}0 endorsements | 6106.00 Influence (Eminence Grise){Environment.NewLine}{Environment.NewLine}WA Vote{Environment.NewLine}GA: UNDECIDED | SC: UNDECIDED{Environment.NewLine}{Environment.NewLine}Links{Environment.NewLine}[Dispatches](https://www.nationstates.net/page=dispatches/nation=testlandia)  |  [Cards Deck](https://www.nationstates.net/page=deck/nation=testlandia)  |  [Challenge](https://www.nationstates.net/page=challenge?entity_name=testlandia)");
        }

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            var options = new Mock<IOptions<AppSettings>>(MockBehavior.Strict);
            options.SetupGet(m => m.Value).Returns(new AppSettings() { SeperatorChar = '$', Locale = "en-US" });
            serviceCollection.AddSingleton<IRequestDispatcher, TestRequestDispatcher>();
            serviceCollection.AddSingleton<IResponseBuilder, ConsoleResponseBuilder>();
            serviceCollection.AddSingleton(typeof(IOptions<AppSettings>), options.Object);
            Program.ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}