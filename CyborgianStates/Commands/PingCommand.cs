using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.Commands
{
    public class PingCommand : ICommand
    {
        public CommandResponse Execute(Message message)
        {
            return new CommandResponse(CommandStatus.Success, "Pong !");
        }
    }
}
