using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyborgianStates
{
    public class BotService : IBotService
    {
        readonly IMessageHandler _messageHandler;
        readonly ILogger _logger;
        readonly IUserRepository _userRepo;
        public BotService(IMessageHandler messageHandler)
        {
            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));
            _messageHandler = messageHandler;
            _logger = ApplicationLogging.CreateLogger(typeof(BotService));
            _userRepo = Program.ServiceProvider.GetService<IUserRepository>();
        }
        public bool IsRunning { get; private set; }

        public async Task InitAsync()
        {
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            _messageHandler.MessageReceived += async (s, e) => await ProgressMessage(e).ConfigureAwait(false);
            await _messageHandler.InitAsync().ConfigureAwait(false);
        }

        private async Task ProgressMessage(MessageReceivedEventArgs e)
        {
            if (IsRunning)
            {
                if (await IsRelevantAsync(e.Message).ConfigureAwait(false))
                {
                    var result = await CommandHandler.Execute(e.Message).ConfigureAwait(false);
                    if (result == null)
                    {
                        _logger.LogError($"Unknown command trigger {e.Message.Content}");
                    }
                }
            }
        }

        private async Task<bool> IsRelevantAsync(Message message)
        {
            if (!await _userRepo.IsUserInDbAsync(message.AuthorId).ConfigureAwait(false))
            {
                await _userRepo.AddUserToDbAsync(message.AuthorId).ConfigureAwait(false);
            }
            var value = !string.IsNullOrWhiteSpace(message.Content);
            if (AppSettings.Configuration == "development")
            {
                return value &&
                    (message.AuthorId == 0 || 
                    await _userRepo.IsBotAdminAsync(message.AuthorId).ConfigureAwait(false) ||
                    await _userRepo.IsAllowedAsync("ExecuteDevCommands", message.AuthorId).ConfigureAwait(false));
            }
            else
            {
                return value &&
                    (message.AuthorId == 0 ||
                    await _userRepo.IsBotAdminAsync(message.AuthorId).ConfigureAwait(false) ||
                    await _userRepo.IsAllowedAsync("ExecuteCommands", message.AuthorId).ConfigureAwait(false));
            }
        }

        public async Task RunAsync()
        {
            IsRunning = true;
            await _messageHandler.RunAsync().ConfigureAwait(false);
        }

        public async Task ShutdownAsync()
        {
            await _messageHandler.ShutdownAsync().ConfigureAwait(false);
            IsRunning = false;
        }
    }
}
