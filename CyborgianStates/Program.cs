﻿using CyborgianStates.CommandHandling;
using CyborgianStates.Data;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using CyborgianStates.Repositories;
using CyborgianStates.Services;
using DataAbstractions.Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NetEscapades.Extensions.Logging.RollingFile;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace CyborgianStates
{
    public static class Program
    {
        private static ILauncher Launcher = new Launcher();
        private static IUserInput userInput = new ConsoleInput();
        private static IMessageHandler messageHandler = new ConsoleMessageHandler(userInput);
        internal static string InputChannel { get; set; }
        public static IServiceProvider ServiceProvider { get; set; }

        public static async Task Main()
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                ServiceProvider = ConfigureServices();
                Console.CancelKeyPress += async (s, e) => await Launcher.ShutdownAsync().ConfigureAwait(false);
                await Launcher.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"A fatal error caused the bot to crash: {ex}");
            }
        }

        public static void SetLauncher(ILauncher launcher)
        {
            Launcher = launcher;
        }

        public static void SetUserInput(IUserInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            userInput = input;
        }

        public static void SetMessageHandler(IMessageHandler handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            messageHandler = handler;
        }

        public static IServiceProvider ConfigureServices()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            string configurationName = "production";
#if DEBUG
            configurationName = "development";
#endif
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile($"appsettings.{configurationName}.json", false, true)
                .Build();
            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration.GetSection("Configuration"));
            var loggeroptions = new FileLoggerOptions() { FileName = "bot-", Extension = "log", RetainedFileCountLimit = null, Periodicity = PeriodicityOptions.Daily };
            serviceCollection.AddSingleton(typeof(IConfiguration), configuration);
            ConfigureLogging(serviceCollection, configuration, loggeroptions);
            // add services
            var channel = configuration.GetSection("Configuration").GetSection("InputChannel").Value;
            if (string.IsNullOrWhiteSpace(InputChannel))
            {
                InputChannel = channel;
            }
            if (InputChannel == "Console")
            {
                serviceCollection.AddSingleton(typeof(IUserInput), userInput);
                serviceCollection.AddSingleton(typeof(IMessageHandler), messageHandler);
                serviceCollection.AddTransient<IResponseBuilder, ConsoleResponseBuilder>();
            }
            else if (InputChannel == "Discord")
            {
                serviceCollection.AddSingleton(typeof(DiscordClientWrapper), new DiscordClientWrapper());
                serviceCollection.AddSingleton<IMessageHandler, DiscordMessageHandler>();
                serviceCollection.AddTransient<IResponseBuilder, DiscordResponseBuilder>();
            }
            else
            {
                throw new InvalidOperationException($"Unknown InputChannel '{InputChannel}'");
            }
            serviceCollection.AddSingleton<IRequestDispatcher, RequestDispatcher>();
            serviceCollection.AddSingleton<IHttpDataService, HttpDataService>();
            serviceCollection.AddSingleton<IBotService, BotService>();
            serviceCollection.AddSingleton<DbConnection, SqliteConnection>();
            serviceCollection.AddSingleton<IDataAccessor, DataAccessor>();
            serviceCollection.AddSingleton<IUserRepository, UserRepository>();
            serviceCollection.AddSingleton<ISqlProvider, SqliteSqlProvider>();
            return serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureLogging(ServiceCollection serviceCollection, IConfiguration configuration, FileLoggerOptions loggerOptions)
        {
            serviceCollection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
                loggingBuilder.AddConsole();
                loggingBuilder.AddFile(options => options = loggerOptions);
            });
            serviceCollection.AddSingleton(typeof(ILoggerFactory), ApplicationLogging.Factory);
            serviceCollection.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }
    }
}