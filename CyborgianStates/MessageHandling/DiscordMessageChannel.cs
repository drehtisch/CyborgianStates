using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using Discord.WebSocket;

namespace CyborgianStates.MessageHandling
{
    public class DiscordMessageChannel : IMessageChannel
    {
        private readonly ISocketMessageChannel _channel;

        public DiscordMessageChannel(ISocketMessageChannel channel, bool isPrivate)
        {
            _channel = channel;
            IsPrivate = isPrivate;
        }

        public bool IsPrivate { get; }

        public Task ReplyToAsync(Message message, string content)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            throw new NotImplementedException();
        }

        public Task ReplyToAsync(Message message, CommandResponse response)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (response is null)
                throw new ArgumentNullException(nameof(response));
            throw new NotImplementedException();
        }

        public Task ReplyToAsync(Message message, string content, bool isPublic)
        {
            throw new NotImplementedException();
        }

        public Task ReplyToAsync(Message message, CommandResponse response, bool isPublic)
        {
            throw new NotImplementedException();
        }

        public Task WriteToAsync(CommandResponse response)
        {
            if (response is null)
                throw new ArgumentNullException(nameof(response));
            throw new NotImplementedException();
        }

        public Task WriteToAsync(string content)
        {
            throw new NotImplementedException();
        }
    }
}