using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyborgianStates.CommandHandling
{
    public class CommandHandler
    {
        private readonly List<CommandDefinition> definitions = new List<CommandDefinition>();
        public int Count { get => definitions.Count; }
        public void Register(CommandDefinition definition)
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

        public ICommand Resolve(string trigger)
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

        public CommandResponse Execute(string trigger, params string[] parameters)
        {
            var command = Resolve(trigger);
            return command.Execute(parameters);
        }
    }
}
