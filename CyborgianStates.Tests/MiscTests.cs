using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace CyborgianStates.Tests
{
    public class MiscTests
    {
        [Fact]
        public void TestGetEventId()
        {
            var res = Helpers.GetEventIdByType(Enums.LoggingEvent.GetNationStats);
            res.Should().BeOfType<EventId>();
            res.Id.Should().Be(10300);
            res.Name.Should().Be("GetNationStats");
        }

        [Fact]
        public void TestGetLoggerByName()
        {
            ILogger logger = ApplicationLogging.CreateLogger("TestLogger");
            Type type = logger.GetType();
            type.Name.Should().Be("Logger");
            type.FullName.Should().Be("Microsoft.Extensions.Logging.Logger");
        }

        [Fact]
        public void TestGetLoggerByType()
        {
            ILogger logger = ApplicationLogging.CreateLogger(typeof(Program));
            Type type = logger.GetType();
            type.Name.Should().Be("Logger");
            type.FullName.Should().Be("Microsoft.Extensions.Logging.Logger");
        }

        [Fact]
        public async Task TestHttpExtensionsReadXml()
        {
            using (var res = new HttpResponseMessage(HttpStatusCode.OK))
            {
                res.Content = new StringContent("<test>test</test>");
                var ret = await res.ReadXmlAsync().ConfigureAwait(false);
                ret.Should().BeOfType<XmlDocument>();
            }
            using (var res = new HttpResponseMessage(HttpStatusCode.OK))
            {
                res.Content = new StringContent("<test>test</test");
                await Assert.ThrowsAsync<ApplicationException>(async () => { await res.ReadXmlAsync().ConfigureAwait(false); }).ConfigureAwait(false);
            }
            using (var res = new HttpResponseMessage(HttpStatusCode.NotFound))
            {
                var ret = await res.ReadXmlAsync().ConfigureAwait(false);
                ret.Should().BeNull();
            }
            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await HttpExtensions.ReadXmlAsync(null).ConfigureAwait(false); }).ConfigureAwait(false);
        }

        [Fact]
        public void TestHttpExtensionsUserAgent()
        {
            Assert.Throws<ArgumentNullException>(() => HttpExtensions.AddCyborgianStatesUserAgent(null, "", ""));
        }

        [Fact]
        public void TestIdMethods()
        {
            var res = Helpers.ToID("Hello World");
            res.Should().Be("hello_world");
            res = Helpers.FromID(res);
            res.Should().Be("hello world");
            res = Helpers.FromID(null);
            res.Should().BeNull();
            res = Helpers.ToID(null);
            res.Should().BeNull();
        }

        [Fact]
        public async Task TestLauncher()
        {
            var serviceCollection = new ServiceCollection();
            var messageHandler = new Mock<IMessageHandler>(MockBehavior.Strict);
            var botService = new Mock<IBotService>(MockBehavior.Strict);
            botService.Setup(m => m.InitAsync()).Returns(Task.CompletedTask);
            botService.Setup(m => m.RunAsync()).Returns(Task.CompletedTask);
            botService.Setup(m => m.ShutdownAsync()).Returns(Task.CompletedTask);
            serviceCollection.AddSingleton(typeof(IMessageHandler), messageHandler.Object);
            serviceCollection.AddSingleton(typeof(IBotService), botService.Object);
            serviceCollection.AddSingleton<IRequestDispatcher, RequestDispatcher>();
            Launcher launcher = new Launcher();
            Program.ServiceProvider = serviceCollection.BuildServiceProvider();
            await launcher.RunAsync().ConfigureAwait(false);
            launcher.IsRunning.Should().BeTrue();
            botService.Verify(m => m.RunAsync(), Times.Once);
            var envMock = new Mock<BotEnvironment>(MockBehavior.Strict);
            envMock.Setup(m => m.Exit(It.IsAny<int>()));
            launcher.SetEnv(envMock.Object);
            await launcher.ShutdownAsync();
        }

        [Fact]
        public void TestLogMessageBuilder()
        {
            var res = Helpers.GetEventIdByType(Enums.LoggingEvent.GetNationStats);
            res.Should().BeOfType<EventId>();
            res.Id.Should().Be(10300);
            res.Name.Should().Be("GetNationStats");
            var logString = LogMessageBuilder.Build(res, "Test");
            logString.Should().Be("[10300] Test");
        }

        [Fact]
        public async Task TestMain()
        {
            Mock<ILauncher> mock = new Mock<ILauncher>(MockBehavior.Strict);
            mock.SetupGet(l => l.IsRunning).Returns(true);
            mock.Setup(m => m.RunAsync()).Returns(Task.CompletedTask);
            ILauncher launcher = mock.Object;
            Program.SetLauncher(launcher);
            IMessageHandler messageHandler = new Mock<IMessageHandler>(MockBehavior.Strict).Object;
            IUserInput userInput = new Mock<IUserInput>(MockBehavior.Strict).Object;
            Assert.Throws<ArgumentNullException>(() => Program.SetUserInput(null));
            Assert.Throws<ArgumentNullException>(() => Program.SetMessageHandler(null));
            Program.SetUserInput(userInput);
            Program.SetMessageHandler(messageHandler);
            await Program.Main().ConfigureAwait(false);
            mock.Verify(l => l.RunAsync(), Times.Once);
            launcher.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void TestConfigureServicesDiscordPath()
        {
            Program.InputChannel = "Discord";
            Program.ConfigureServices();
            Program.InputChannel = "test";
            Assert.Throws<InvalidOperationException>(() => Program.ConfigureServices());
            Program.InputChannel = "Console";
            Program.ConfigureServices();
            Program.InputChannel = string.Empty;
        }

        [Fact]
        public async Task TestMainWithLauncherFailure()
        {
            Mock<ILauncher> mock = new Mock<ILauncher>(MockBehavior.Strict);
            mock.SetupGet(l => l.IsRunning).Returns(true);
            mock.Setup(m => m.RunAsync()).Returns(() => throw new Exception("Unit Test: Forced Launcher Failure"));
            ILauncher launcher = mock.Object;
            Program.SetLauncher(launcher);
            IMessageHandler messageHandler = new Mock<IMessageHandler>(MockBehavior.Strict).Object;
            IUserInput userInput = new Mock<IUserInput>(MockBehavior.Strict).Object;
            Program.SetUserInput(userInput);
            Program.SetMessageHandler(messageHandler);
            await Program.Main().ConfigureAwait(false);
            mock.Verify(l => l.RunAsync(), Times.Once);
            launcher.IsRunning.Should().BeTrue();
        }
    }
}