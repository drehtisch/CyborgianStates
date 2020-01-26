using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CyborgianStates.CommandHandling
{
    public class CommandDefinition
    {
        public CommandDefinition(Type type, List<string> triggers)
        {
            Trigger = new ReadOnlyCollection<string>(triggers);
            Type = type;
        }
        public Type Type { get; }
        public ReadOnlyCollection<string> Trigger { get; }
    }
}
