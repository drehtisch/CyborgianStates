using CyborgianStates.CommandHandling;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface ICommand
    {
        Task<CommandResponse> Execute(Message message);
    }
}
