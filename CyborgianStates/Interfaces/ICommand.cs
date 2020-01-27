using CyborgianStates.CommandHandling;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.Interfaces
{
    public interface ICommand
    {
        CommandResponse Execute(params string[] parameters);
    }
}
