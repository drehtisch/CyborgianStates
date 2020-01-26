using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.Interfaces
{
    public interface ICommand
    {
        string Execute(Dictionary<string, object> parameter);
    }
}
