using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyborgianStates.Services
{
    public class BackgroundServiceRegistry : IBackgroundServiceRegistry
    {

        private List<IBackgroundService> _backgroundServices = new List<IBackgroundService>();
        private ISchedulerFactory _factory;
        private IScheduler _scheduler;
        public BackgroundServiceRegistry()
        {
            _factory = new StdSchedulerFactory();
        }
        public void Register(IBackgroundService service)
        {
            _backgroundServices.Add(service);
        }
        public async Task ShutdownAsync() => await _scheduler.Shutdown().ConfigureAwait(false);
        public async Task StartAsync()
        {
            _scheduler = await _factory.GetScheduler().ConfigureAwait(false);
            await _scheduler.Start().ConfigureAwait(false);
            foreach (var service in _backgroundServices)
            {   
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity(service.Identity, service.Group)
                    .WithSchedule(CronScheduleBuilder.CronSchedule(service.CronSchedule).InTimeZone(service.TimeZone))
                    .ForJob(service.Identity, service.Group).Build();

                var type = service.GetType();
                IJobDetail job = JobBuilder.Create(type)
                    .WithIdentity(service.Identity, service.Group)
                    .Build();
                var time = await _scheduler.ScheduleJob(job, trigger).ConfigureAwait(false);
                if (service.DoStartNow)
                {
                    await _scheduler.TriggerJob(new JobKey(service.Identity,service.Group)).ConfigureAwait(false);
                }
                Console.WriteLine(time);
            }
        }
    }

    public class DummyService : IBackgroundService
    {
        ILogger _logger;
        public DummyService()
        {
            _logger = ApplicationLogging.CreateLogger(typeof(DummyService));
        }
        public string CronSchedule => "0 0/1 * ? * * *";

        public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;

        public bool DoStartNow => true;

        public string Identity => "Dummy";

        public string Group => "DummyGroup";

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Executing Dummy Service. Lol");
            return Task.CompletedTask;
        }
    }
}
