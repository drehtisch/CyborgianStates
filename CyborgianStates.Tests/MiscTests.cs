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
            Assert.IsType<EventId>(res);
            Assert.Equal(10300, res.Id);
            Assert.Equal("GetNationStats", res.Name);
        }

        [Fact]
        public void TestGetLoggerByName()
        {
            ILogger logger = ApplicationLogging.CreateLogger("TestLogger");
            Type type = logger.GetType();
            Assert.Equal("Logger", type.Name);
            Assert.Equal("Microsoft.Extensions.Logging.Logger", type.FullName);
        }

        [Fact]
        public void TestGetLoggerByType()
        {
            ILogger logger = ApplicationLogging.CreateLogger(typeof(Program));
            Type type = logger.GetType();
            Assert.Equal("Logger", type.Name);
            Assert.Equal("Microsoft.Extensions.Logging.Logger", type.FullName);
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
            Assert.Equal("hello_world", res);
            res = Helpers.FromID(res);
            Assert.Equal("hello world", res);
            res = Helpers.FromID(null);
            Assert.Null(res);
            res = Helpers.ToID(null);
            Assert.Null(res);
        }

        [Fact]
        public async Task TestLauncher()
        {
            var serviceCollection = new ServiceCollection();
            var messageHandler = new Mock<IMessageHandler>(MockBehavior.Strict);
            var botService = new Mock<IBotService>(MockBehavior.Strict);
            botService.Setup(m => m.InitAsync()).Returns(Task.CompletedTask);
            botService.Setup(m => m.RunAsync()).Returns(Task.CompletedTask);
            serviceCollection.AddSingleton(typeof(IMessageHandler), messageHandler.Object);
            serviceCollection.AddSingleton(typeof(IBotService), botService.Object);
            serviceCollection.AddSingleton<IRequestDispatcher, RequestDispatcher>();
            Launcher launcher = new Launcher();
            Program.ServiceProvider = serviceCollection.BuildServiceProvider();
            await launcher.RunAsync().ConfigureAwait(false);
            Assert.True(launcher.IsRunning);
            botService.Verify(m => m.RunAsync(), Times.Once);
        }

        [Fact]
        public void TestLogMessageBuilder()
        {
            var res = Helpers.GetEventIdByType(Enums.LoggingEvent.GetNationStats);
            Assert.IsType<EventId>(res);
            Assert.Equal(10300, res.Id);
            Assert.Equal("GetNationStats", res.Name);
            var logString = LogMessageBuilder.Build(res, "Test");
            Assert.Equal("[10300] Test", logString);
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
            Assert.True(launcher.IsRunning);
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
            Assert.True(launcher.IsRunning);
        }
    }
}