using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace CyborgianStates
{
    public class BotService : IBotService
    {
        readonly IMessageHandler _messageHandler;
        public BotService(IMessageHandler messageHandler)
        {
            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));
            _messageHandler = messageHandler;
        }
        public bool IsRunning { get; private set; }

        public async Task InitAsync()
        {
            _messageHandler.MessageReceived += async (s, e) => await ProgressMessage(e).ConfigureAwait(false);
            await _messageHandler.InitAsync().ConfigureAwait(false);
        }

        private async Task ProgressMessage(MessageReceivedEventArgs e)
        {
            if (IsRunning)
            {
                if (e.Message.AuthorId == 0)
                {
                    await CommandHandler.Execute(e.Message).ConfigureAwait(false);
                }
            }
        }
        public async Task RunAsync()
        {
            await _messageHandler.RunAsync().ConfigureAwait(false);
            IsRunning = true;
        }

        public async Task ShutdownAsync()
        {
            await _messageHandler.ShutdownAsync().ConfigureAwait(false);
            IsRunning = false;
        }
    }
}
