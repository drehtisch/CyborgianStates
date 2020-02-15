using CyborgianStates.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Test
{
    public class MiscTests
    {
        [Fact]
        public void TestMain()
        {
            Mock<ILauncher> mock = new Mock<ILauncher>();
            mock.SetupGet(l => l.IsRunning).Returns(true);
            ILauncher launcher = mock.Object;
            Program.SetLauncher(launcher);
            IMessageHandler messageHandler = new Mock<IMessageHandler>().Object;
            IUserInput userInput = new Mock<IUserInput>().Object;
            Assert.Throws<ArgumentNullException>(() => Program.SetUserInput(null));
            Assert.Throws<ArgumentNullException>(() => Program.SetMessageHandler(null));
            Program.SetUserInput(userInput);
            Program.SetMessageHandler(messageHandler);
            Program.Main();
            mock.Verify(l => l.RunAsync(), Times.Once);
            Assert.True(launcher.IsRunning);
        }
        [Fact]
        public async Task TestLauncher()
        {
            var serviceCollection = new ServiceCollection();
            var messageHandler = new Mock<IMessageHandler>();
            var botService = new Mock<IBotService>();
            serviceCollection.AddSingleton(typeof(IMessageHandler), messageHandler.Object);
            serviceCollection.AddSingleton(typeof(IBotService), botService.Object);
            Program.ServiceProvider = serviceCollection.BuildServiceProvider();
            Launcher launcher = new Launcher();
            await launcher.RunAsync().ConfigureAwait(false);
            Assert.True(launcher.IsRunning);
            botService.Verify(m => m.RunAsync(), Times.Once);
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
    }
}
