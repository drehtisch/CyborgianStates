using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.MessageHandling
{
    public class ConsoleMessageChannel : IMessageChannel
    {
        ILogger _logger;
        public ConsoleMessageChannel(bool isPrivate)
        {
            _logger = ApplicationLogging.CreateLogger(typeof(ConsoleMessageChannel));
            IsPrivate = isPrivate;
        }
        public bool IsPrivate { get; }
        public async Task WriteToAsync(bool responseIsPublic, CommandResponse response)
        {
            if(response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }
            _logger.LogInformation(response.Content);
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
