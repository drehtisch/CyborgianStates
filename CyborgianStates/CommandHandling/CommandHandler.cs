using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyborgianStates.CommandHandling
{
    public static class CommandHandler
    {
        private static readonly List<CommandDefinition> definitions = new List<CommandDefinition>();
        public static int Count { get => definitions.Count; }
        public static void Register(CommandDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            if (definition.Trigger == null || definition.Type == null || definition.Trigger.Count == 0)
            {
                throw new InvalidOperationException("The passed CommandDefinition was invalid. Check that Trigger and Type are not null and that >0 Triggers are defined.");
            }
            if (!typeof(ICommand).IsAssignableFrom(definition.Type))
            {
                throw new InvalidOperationException("The passed CommandDefinition was invalid. The provided Type is required to implement the CyborgianStates.Interfaces.ICommand Interface.");
            }
            definitions.Add(definition);
        }

        public static ICommand Resolve(string trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger))
            {
                throw new ArgumentNullException(nameof(trigger));
            }
            var def = definitions.Where(cd => cd.Trigger.Contains(trigger)).FirstOrDefault();
            if (def != null)
            {
                ICommand instance = (ICommand)Activator.CreateInstance(def.Type);
                return instance;
            }
            else
            {
                return null;
            }
        }

        public static CommandResponse Execute(Message message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            var trigger = message.Content.Contains(' ', StringComparison.InvariantCulture) ? message.Content.Split(' ')[0] : message.Content;
            var command = Resolve(trigger);
            return command.Execute(message);
        }

        public static void Clear()
        {
            definitions.Clear();
        }
    }
}
