using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Commands
{
    public class PingCommand : ICommand
    {
        private readonly ILogger _logger;
        private CancellationToken token;

        public PingCommand()
        {
            _logger = ApplicationLogging.CreateLogger(typeof(PingCommand));
        }

        public async Task<CommandResponse> Execute(Message message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            _logger.LogDebug($"- Ping -");
            var response = new CommandResponse(CommandStatus.Success, "Pong ! - Cyborgian States v4");
            await message.Channel.ReplyToAsync(message, response).ConfigureAwait(false);
            return response;
        }

        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            token = cancellationToken;
        }
    }
}