using CyborgianStates.Interfaces;
using System;
using System.Threading.Tasks;

namespace CyborgianStates.MessageHandling
{
    public class ConsoleMessageHandler : IMessageHandler
    {
        IUserInput _input;
        public ConsoleMessageHandler(IUserInput input)
        {
            _input = input;
        }

        public bool IsRunning { get; private set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public Task InitAsync()
        {
            Console.WriteLine("ConsoleMessageHandler Init");
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
                Task.Delay(1000).Wait();
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
