using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public Task RunAsync()
        {
            IsRunning = true;
            while (IsRunning)
            {
                var input = Console.ReadLine();
                MessageReceived(this, new MessageReceivedEventArgs(new Message(0, input, new ConsoleMessageChannel(true))));
            }
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            throw new NotImplementedException();
        }
    }
}
