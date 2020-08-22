using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using Discord.Commands;
using Discord.WebSocket;

namespace CyborgianStates.MessageHandling
{
    public class DiscordMessageChannel : IMessageChannel
    {
        private readonly ISocketMessageChannel _channel;
        private Discord.IMessageChannel currentChannel;

        public DiscordMessageChannel(ISocketMessageChannel channel, bool isPrivate)
        {
            _channel = channel;
            currentChannel = _channel;
            IsPrivateChannel = isPrivate;
        }

        private bool IsPrivateChannel;

        public async Task ReplyToAsync(Message message, string content)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            await ReplyToAsync(message, content, true).ConfigureAwait(false);
        }

        public async Task ReplyToAsync(Message message, CommandResponse response)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (response is null)
                throw new ArgumentNullException(nameof(response));
            await ReplyToAsync(message, response.Content, true).ConfigureAwait(false);
        }

        public async Task ReplyToAsync(Message message, string content, bool isPublic)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (!isPublic && !IsPrivateChannel)
            {
                var Context = message.MessageObject as SocketCommandContext;
                currentChannel = await Context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
            }
            else
            {
                currentChannel = _channel;
            }
            await WriteToAsync(content).ConfigureAwait(false);
        }

        public async Task ReplyToAsync(Message message, CommandResponse response, bool isPublic)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (response is null)
                throw new ArgumentNullException(nameof(response));
            await ReplyToAsync(message, response.Content, isPublic).ConfigureAwait(false);
        }

        public async Task WriteToAsync(CommandResponse response)
        {
            if (response is null)
                throw new ArgumentNullException(nameof(response));
            await WriteToAsync(response.Content).ConfigureAwait(false);
        }

        public async Task WriteToAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content));
            await currentChannel.SendMessageAsync(content).ConfigureAwait(false);
        }
    }
}