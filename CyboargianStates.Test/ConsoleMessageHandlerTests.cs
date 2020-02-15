using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CyboargianStates.Test
{
    public class ConsoleMessageHandlerTests
    {
        [Fact]
        public async Task ConsoleMessageHandlerTest()
        {
            var mockInput = new Mock<IUserInput>();
            mockInput.Setup(m => m.GetInput()).Returns("ping");
            IUserInput userInput = mockInput.Object;
            ConsoleMessageHandler consoleMessageHandler = new ConsoleMessageHandler(userInput);
            await consoleMessageHandler.InitAsync().ConfigureAwait(false);
            var task = Task.Run(async () => await consoleMessageHandler.RunAsync().ConfigureAwait(false));
            await Task.Delay(1000).ConfigureAwait(false);
            Assert.True(consoleMessageHandler.IsRunning);
            mockInput.Verify(m => m.GetInput(), Times.AtLeastOnce);
            await consoleMessageHandler.ShutdownAsync().ConfigureAwait(false);
            task.Wait();
            Assert.False(consoleMessageHandler.IsRunning);
        }

        [Fact]
        public async Task ConsoleMessageHandlerTest_WithEmptyMessage()
        {
            var mockInput = new Mock<IUserInput>();
            mockInput.Setup(m => m.GetInput()).Returns(string.Empty);
            IUserInput userInput = mockInput.Object;
            ConsoleMessageHandler consoleMessageHandler = new ConsoleMessageHandler(userInput);
            await consoleMessageHandler.InitAsync().ConfigureAwait(false);
            var task = Task.Run(async () => await consoleMessageHandler.RunAsync().ConfigureAwait(false));
            await Task.Delay(1000).ConfigureAwait(false);
            Assert.True(consoleMessageHandler.IsRunning);
            mockInput.Verify(m => m.GetInput(), Times.AtLeastOnce);
            await consoleMessageHandler.ShutdownAsync().ConfigureAwait(false);
            task.Wait();
            Assert.False(consoleMessageHandler.IsRunning);
        }
    }
}
