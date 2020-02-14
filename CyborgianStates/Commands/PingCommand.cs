using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Commands
{
    public class PingCommand : ICommand
    {
        public async Task<CommandResponse> Execute(Message message)
        {
            if(message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            Console.WriteLine($"- Ping on Private Channel: {message.Channel.IsPrivate}");
            var response = new CommandResponse(CommandStatus.Success, "Pong !");
            await message.Channel.WriteToAsync(true, response).ConfigureAwait(false);
            return response;
        }
    }
}
