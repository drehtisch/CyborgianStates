using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.Commands
{
    public class PingCommand : ICommand
    {
        public CommandResponse Execute(params string[] parameters)
        {
            return new CommandResponse(CommandStatus.Success, "Pong !");
        }
    }
}
