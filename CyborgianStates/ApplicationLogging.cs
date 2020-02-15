using Microsoft.Extensions.Logging;
using NetEscapades.Extensions.Logging.RollingFile;
using System;

namespace CyborgianStates
{
    public static class ApplicationLogging
    {
        private static ILoggerFactory _factory = null;

        public static ILoggerFactory CreateLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole()
                    .AddFile(opt =>
                    {
                        opt.RetainedFileCountLimit = null;
                        opt.Periodicity = PeriodicityOptions.Daily;
                    });
            });
        }
        public static ILoggerFactory Factory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = CreateLoggerFactory();
                }
                return _factory;
            }
        }
        public static ILogger CreateLogger(Type type) => Factory.CreateLogger(type);
        public static ILogger CreateLogger(string name) => Factory.CreateLogger(name);
    }

}
