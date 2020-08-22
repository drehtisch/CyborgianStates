using System;
using System.Threading;
using System.Threading.Tasks;
using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CyborgianStates.MessageHandling
{
    public class DiscordMessageHandler : IMessageHandler
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private CommandService _commandService;
        private readonly AppSettings _settings;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public DiscordMessageHandler(ILogger<DiscordMessageHandler> logger, IOptions<AppSettings> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            _logger = logger;
            _client = new DiscordSocketClient();
            _settings = options.Value;
        }

        public bool IsRunning { get; private set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public async Task InitAsync()
        {
            _logger.LogInformation("-- DiscordMessageHandler Init --");
            _commandService = new CommandService(new CommandServiceConfig
            {
                SeparatorChar = _settings.SeperatorChar,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
                LogLevel = LogSeverity.Debug
            });
            //await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), Program.ServiceProvider).ConfigureAwait(false);
            await _client.LoginAsync(TokenType.Bot, _settings.DiscordBotLoginToken).ConfigureAwait(false);
            SetupDiscordEvents();
        }

        private void SetupDiscordEvents()
        {
            _client.Connected += Discord_Connected;
            _client.Disconnected += Discord_Disconnected;
            _client.Log += Discord_Log;
            _client.MessageReceived += Discord_MessageReceived;
            _client.LoggedIn += Discord_LoggedIn;
            _client.LoggedOut += Discord_LoggedOut;
            _client.Ready += Discord_Ready;
        }

        private Task Discord_Ready()
        {
            _logger.LogInformation("--- Discord Client Ready ---");
            IsRunning = true;
            return Task.CompletedTask;
        }

        private Task Discord_LoggedOut()
        {
            _logger.LogInformation("--- Bot logged out ---");
            return Task.CompletedTask;
        }

        private Task Discord_LoggedIn()
        {
            _logger.LogInformation("--- Bot logged in ---");
            return Task.CompletedTask;
        }

        private Task Discord_MessageReceived(SocketMessage arg)
        {
            if (arg is SocketUserMessage message)
            {
                if (message.Content.StartsWith(_settings.SeperatorChar))
                {
                    var msgContent = message.Content.Substring(1, message.Content.Length - 1);
                    var context = new SocketCommandContext(_client, message);
                    var isPrivate = context.IsPrivate;
                    MessageReceived?.Invoke(this,
                        new MessageReceivedEventArgs(
                            new Message(
                                message.Author.Id,
                                msgContent,
                                new DiscordMessageChannel(message.Channel, isPrivate),
                                context
                    )));
                }
            }
            return Task.CompletedTask;
        }

        private Task Discord_Log(LogMessage arg)
        {
            var id = Helpers.GetEventIdByType(LoggingEvent.DiscordLogEvent);
            string message = LogMessageBuilder.Build(id, $"[{arg.Source}] {arg.Message}");
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(id, message);
                    break;

                case LogSeverity.Error:
                    _logger.LogError(id, message);
                    break;

                case LogSeverity.Warning:
                    _logger.LogWarning(id, message);
                    break;

                case LogSeverity.Info:
                    _logger.LogInformation(id, message);
                    break;

                default:
                    _logger.LogDebug(id, $"Severity: {arg.Severity} {message}");
                    break;
            }
            return Task.CompletedTask;
        }

        private Task Discord_Disconnected(Exception arg)
        {
            _logger.LogInformation(arg, "--- Disconnected from Discord ---");
            return Task.CompletedTask;
        }

        private Task Discord_Connected()
        {
            _logger.LogInformation("--- Connected to Discord ---");
            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            await _client.StartAsync().ConfigureAwait(false);
            IsRunning = true;
            await Task.Delay(-1, cancellationTokenSource.Token).ConfigureAwait(false);
        }

        public async Task ShutdownAsync()
        {
            await _client.LogoutAsync().ConfigureAwait(false);
            await _client.StopAsync().ConfigureAwait(false);
            _client.Dispose();
            IsRunning = false;
            cancellationTokenSource.Cancel();
        }
    }
}