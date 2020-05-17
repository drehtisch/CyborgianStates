using CyborgianStates.MessageHandling;
using CyborgianStates.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetEscapades.Extensions.Logging.RollingFile;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CyborgianStates.CommandHandling;
using CyborgianStates.Services;
using System.Threading.Tasks;

namespace CyborgianStates
{
    public static class Program
    {
        static ILauncher Launcher = new Launcher();
        static IUserInput userInput = new ConsoleInput();
        static IMessageHandler messageHandler = new ConsoleMessageHandler(userInput);

        public static IServiceProvider ServiceProvider { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Required to be able to log fatal exceptions causing the bot to crash.")]
        public static async Task Main()
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                ServiceProvider = serviceCollection.BuildServiceProvider();
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

        private static void ConfigureServices(ServiceCollection serviceCollection)
        {

            string configurationName = "production";
#if DEBUG
            configurationName = "development";
#endif
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{configurationName}.json", false, true)
                .Build();
            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration.GetSection("Configuration"));
            var loggeroptions = new FileLoggerOptions() { FileName = "bot-", Extension = "log", RetainedFileCountLimit = null, Periodicity = PeriodicityOptions.Daily };
            ConfigureLogging(serviceCollection);
            // add services
            serviceCollection.AddSingleton(typeof(IUserInput), userInput);
            serviceCollection.AddSingleton(typeof(IMessageHandler), messageHandler);
            serviceCollection.AddSingleton<IRequestDispatcher, RequestDispatcher>();
            serviceCollection.AddSingleton<IHttpDataService, HttpDataService>();
            serviceCollection.AddSingleton<IBotService, BotService>();
        }

        private static void ConfigureLogging(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(typeof(ILoggerFactory), ApplicationLogging.Factory);
            serviceCollection.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }
    }
}
