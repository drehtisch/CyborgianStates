using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CyborgianStates
{
    public class BotService : IBotService
    {
        readonly IMessageHandler _messageHandler;
        ILogger _logger;
        public BotService(IMessageHandler messageHandler)
        {
            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));
            _messageHandler = messageHandler;
            _logger = ApplicationLogging.CreateLogger(typeof(BotService));
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
                    var result = await CommandHandler.Execute(e.Message).ConfigureAwait(false);
                    if(result == null)
                    {
                        _logger.LogError($"Unknown command trigger {e.Message.Content}");
                    }
                }
            }
        }
        public async Task RunAsync()
        {
            IsRunning = true;
            await _messageHandler.RunAsync().ConfigureAwait(false);
        }

        public async Task ShutdownAsync()
        {
            await _messageHandler.ShutdownAsync().ConfigureAwait(false);
            IsRunning = false;
        }
    }
}
