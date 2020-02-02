using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IMessageHandler
    {
        bool IsRunning { get; }
        Task InitAsync();
        Task RunAsync();
        Task ShutdownAsync();
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }

    public class MessageReceivedEventArgs
    {
        public MessageReceivedEventArgs(Message message)
        {
            Message = message;
        }
        public Message Message { get; }
    }
}
