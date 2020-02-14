using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.MessageHandling
{
    public class ConsoleMessageHandler : IMessageHandler
    {
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
                if (Console.KeyAvailable)
                {
                    var input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(new Message(0, input, new ConsoleMessageChannel(true))));
                    }
                }
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
