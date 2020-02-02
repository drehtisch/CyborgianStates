using CyborgianStates.CommandHandling;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.Interfaces
{
    public interface ICommand
    {
        CommandResponse Execute(Message message);
    }
}
