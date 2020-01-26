using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.Commands
{
    public class PingCommand : ICommand
    {
        public string Execute(params string[] parameters)
        {
            return "Pong !";
        }
    }
}
