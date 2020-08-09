using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CyborgianStates.CommandHandling
{
    public class CommandDefinition
    {
        public CommandDefinition(Type type, List<string> triggers)
        {
            Trigger = new ReadOnlyCollection<string>(triggers);
            Type = type;
        }

        public ReadOnlyCollection<string> Trigger { get; }
        public Type Type { get; }
    }
}