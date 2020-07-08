using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyborgianStates.Services
{
    public class BotService : IBotService
    {
        readonly IMessageHandler _messageHandler;
        readonly ILogger _logger;
        readonly IUserRepository _userRepo;
        readonly IRequestDispatcher _requestDispatcher;
        readonly IHttpDataService _httpDataService;
        readonly NationStatesApiRequestQueue _nationStatesApiQueue;
        readonly NationStatesApiDataService _nationStatesApiDataService;
        public BotService(IMessageHandler messageHandler)
        {
            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));
            _messageHandler = messageHandler;
            _logger = ApplicationLogging.CreateLogger(typeof(BotService));
            _userRepo = Program.ServiceProvider.GetService<IUserRepository>();
            _requestDispatcher = Program.ServiceProvider.GetService<IRequestDispatcher>();
            _httpDataService = Program.ServiceProvider.GetService<IHttpDataService>();
            _nationStatesApiDataService = new NationStatesApiDataService(_httpDataService);
            _nationStatesApiQueue = new NationStatesApiRequestQueue(_nationStatesApiDataService);
        }
        public bool IsRunning { get; private set; }

        public async Task InitAsync()
        {
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            CommandHandler.Register(new CommandDefinition(typeof(NationStatsCommand), new List<string>() { "nation", "n" }));
            _messageHandler.MessageReceived += async (s, e) => await ProcessMessage(e).ConfigureAwait(false);
            await _requestDispatcher.Register(DataSourceType.NationStatesAPI, _nationStatesApiQueue).ConfigureAwait(false);
            await _messageHandler.InitAsync().ConfigureAwait(false);
        }

        private async Task ProcessMessage(MessageReceivedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogCritical(ex,$"Unexpected error occured while Processing Message -> {e.Message}: ");
            }
        }

        private async Task<bool> IsRelevantAsync(Message message)
        {
            if (message.AuthorId != 0 && !await _userRepo.IsUserInDbAsync(message.AuthorId).ConfigureAwait(false))
            {
                await _userRepo.AddUserToDbAsync(message.AuthorId).ConfigureAwait(false);
            }
            var value = !string.IsNullOrWhiteSpace(message.Content);
            if (AppSettings.Configuration == "development")
            {
                return value &&
                    (message.AuthorId == 0 ||
                    await _userRepo.IsAllowedAsync("Commands.Preview.Execute", message.AuthorId).ConfigureAwait(false));
            }
            else
            {
                return value &&
                    (message.AuthorId == 0 ||
                    await _userRepo.IsAllowedAsync("Commands.Execute", message.AuthorId).ConfigureAwait(false));
            }
        }

        public async Task RunAsync()
        {
            IsRunning = true;
            await _messageHandler.RunAsync().ConfigureAwait(false);
        }

        public async Task ShutdownAsync()
        {
            CommandHandler.Cancel();
            await _messageHandler.ShutdownAsync().ConfigureAwait(false);
            IsRunning = false;
        }
    }
}
