using System;
using System.Threading.Tasks;
using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;

namespace CyborgianStates.MessageHandling
{
    public class ConsoleMessageChannel : IMessageChannel
    {
        private ILogger _logger;

        public ConsoleMessageChannel()
        {
            _logger = ApplicationLogging.CreateLogger(typeof(ConsoleMessageChannel));
        }

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

        public Task WriteToAsync(string content)
        {
            _logger.LogInformation(content);
            return Task.CompletedTask;
        }

        public async Task WriteToAsync(CommandResponse response)
        {
            if (response is null)
                throw new ArgumentNullException(nameof(response));
            await WriteToAsync(response.Content).ConfigureAwait(false);
        }
    }
}