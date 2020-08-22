using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CyborgianStates.MessageHandling
{
    public class ConsoleMessageHandler : IMessageHandler
    {
        private readonly IUserInput _input;
        private readonly ILogger _logger;

        public ConsoleMessageHandler(IUserInput input)
        {
            _input = input;
            _logger = ApplicationLogging.CreateLogger(typeof(ConsoleMessageHandler));
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public bool IsRunning { get; private set; }

        public Task InitAsync()
        {
            _logger.LogInformation("-- ConsoleMessageHandler Init --");
            return Task.CompletedTask;
        }

        public Task RunAsync()
        {
            IsRunning = true;
            while (IsRunning)
            {
                var input = _input.GetInput();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(new Message(0, input, new ConsoleMessageChannel(true))));
                }
                Task.Delay(50).Wait();
            }
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            IsRunning = false;

            return Task.CompletedTask;
        }
    }
}