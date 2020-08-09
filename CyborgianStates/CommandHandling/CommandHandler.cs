using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.CommandHandling
{
    public static class CommandHandler
    {
        private static readonly List<CommandDefinition> definitions = new List<CommandDefinition>();
        private static readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        public static int Count { get => definitions.Count; }

        public static void Cancel()
        {
            tokenSource.Cancel();
        }

        public static void Clear()
        {
            definitions.Clear();
        }

        public static async Task<CommandResponse> Execute(Message message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            string trigger = message.Content.Contains(' ', StringComparison.InvariantCulture) ? message.Content.Split(' ')[0] : message.Content;
            ICommand command = await Resolve(trigger).ConfigureAwait(false);
            if (command != null)
            {
                return await command.Execute(message).ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }

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

        public static async Task<ICommand> Resolve(string trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger))
            {
                throw new ArgumentNullException(nameof(trigger));
            }
            var def = definitions.Where(cd => cd.Trigger.Contains(trigger)).FirstOrDefault();
            if (def != null)
            {
                ICommand instance = (ICommand)Activator.CreateInstance(def.Type);
                instance.SetCancellationToken(tokenSource.Token);
                return await Task.FromResult(instance).ConfigureAwait(false);
            }
            else
            {
                return await Task.FromResult<ICommand>(null).ConfigureAwait(false);
            }
        }
    }
}