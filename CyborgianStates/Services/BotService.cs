using CyborgianStates.CommandHandling;
using CyborgianStates.Commands;
using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Services
{
    public class BotService : IBotService
    {
        private readonly ILogger _logger;
        private readonly IMessageHandler _messageHandler;
        private readonly IRequestDispatcher _requestDispatcher;
        private readonly IUserRepository _userRepo;

        public BotService(IMessageHandler messageHandler, IRequestDispatcher requestDispatcher, IUserRepository userRepository)
        {
            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));
            if (requestDispatcher is null)
                throw new ArgumentNullException(nameof(requestDispatcher));
            if (userRepository is null)
                throw new ArgumentNullException(nameof(requestDispatcher));
            _messageHandler = messageHandler;
            _requestDispatcher = requestDispatcher;
            _userRepo = userRepository;
            _logger = ApplicationLogging.CreateLogger(typeof(BotService));
        }

        public bool IsRunning { get; private set; }

        public async Task InitAsync()
        {
            _logger.LogInformation("BotService Initializing");
            Register();
            _messageHandler.MessageReceived += async (s, e) => await ProcessMessageAsync(e).ConfigureAwait(false);
            await _messageHandler.InitAsync().ConfigureAwait(false);
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("BotService Starting");
            IsRunning = true;
            _requestDispatcher.Start();
            _logger.LogInformation("BotService Running");
            await _messageHandler.RunAsync().ConfigureAwait(false);
        }

        public async Task ShutdownAsync()
        {
            _logger.LogInformation("BotService Shutdown");
            CommandHandler.Cancel();
            _requestDispatcher.Shutdown();
            await _messageHandler.ShutdownAsync().ConfigureAwait(false);
            IsRunning = false;
            _logger.LogInformation("BotService Stopped");
        }

        private static void RegisterCommands()
        {
            CommandHandler.Register(new CommandDefinition(typeof(PingCommand), new List<string>() { "ping" }));
            CommandHandler.Register(new CommandDefinition(typeof(NationStatsCommand), new List<string>() { "nation", "n" }));
        }

        private async Task<bool> IsRelevantAsync(Message message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (message.AuthorId != 0 && !await _userRepo.IsUserInDbAsync(message.AuthorId).ConfigureAwait(false))
            {
                await _userRepo.AddUserToDbAsync(message.AuthorId).ConfigureAwait(false);
            }
            var value = !string.IsNullOrWhiteSpace(message.Content);
            return value &&
                (message.AuthorId == 0 ||
                await _userRepo.IsAllowedAsync(
                    AppSettings.Configuration == "development" ? "Commands.Preview.Execute" : "Commands.Execute",
                    message.AuthorId).ConfigureAwait(false));
        }

        private async Task ProcessMessageAsync(MessageReceivedEventArgs e)
        {
            try
            {
                if (IsRunning)
                {
                    if (await IsRelevantAsync(e.Message).ConfigureAwait(false))
                    {
                        var result = await CommandHandler.ExecuteAsync(e.Message).ConfigureAwait(false);
                        if (result == null)
                        {
                            _logger.LogError($"Unknown command trigger {e.Message.Content}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unexpected error occured while Processing Message -> {e.Message}: ");
            }
        }

        private void Register()
        {
            RegisterCommands();
            var dataService = new NationStatesApiDataService(Program.ServiceProvider.GetService(typeof(IHttpDataService)) as IHttpDataService);
            var queue = new NationStatesApiRequestWorker(dataService);
            _requestDispatcher.Register(DataSourceType.NationStatesAPI, queue);
        }
    }
}