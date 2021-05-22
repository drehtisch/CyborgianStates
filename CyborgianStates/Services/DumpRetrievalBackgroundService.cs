using CyborgianStates.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NationStatesSharp.Interfaces;
using Quartz;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Services
{
    public class DumpRetrievalBackgroundService : IBackgroundService
    {
        private IDumpRetrievalService _dumpRetrievalService;

        public DumpRetrievalBackgroundService()
        {
            _dumpRetrievalService = Program.ServiceProvider.GetRequiredService<IDumpRetrievalService>();
        }

        public string Identity => "Dump Retrieval";

        public string Group => "Dump";

        public string CronSchedule => "0 23 * ? * *";

        public TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

        public bool DoStartNow => true;

        public async Task Execute(IJobExecutionContext context)
        {
            var logger = Log.Logger.ForContext<DumpRetrievalBackgroundService>();
            logger.Information("--- Start Dump Data Update ---");
            var stopWatch = Stopwatch.StartNew();
            if (!_dumpRetrievalService.IsLocalDumpAvailableAndUpToDate(NationStatesSharp.Enums.DumpType.Nations))
            {
                logger.Debug("Download nation dump as stream took {@elapsed} to complete.", stopWatch.Elapsed);
                using (var fileStream = new FileStream("nations.xml.gz", FileMode.Create))
                {
                    using (Stream responseStream = await _dumpRetrievalService.DownloadDumpAsync(NationStatesSharp.Enums.DumpType.Nations, CancellationToken.None).ConfigureAwait(false))
                    {
                        await responseStream.CopyToAsync(fileStream).ConfigureAwait(false);
                    }
                }
                logger.Debug("Writing nation dump from local cache took {@elapsed} to complete.", stopWatch.Elapsed);
            }
            if (!_dumpRetrievalService.IsLocalDumpAvailableAndUpToDate(NationStatesSharp.Enums.DumpType.Regions))
            {
                logger.Debug("Download region dump as stream took {@elapsed} to complete.", stopWatch.Elapsed);
                using (var fileStream = new FileStream("regions.xml.gz", FileMode.Create))
                {
                    using (Stream responseStream = await _dumpRetrievalService.DownloadDumpAsync(NationStatesSharp.Enums.DumpType.Regions, CancellationToken.None).ConfigureAwait(false))
                    {
                        await responseStream.CopyToAsync(fileStream).ConfigureAwait(false);
                    }
                }
                logger.Debug("Writing region dump to local cache took {@elapsed} to complete.", stopWatch.Elapsed);
            }
            logger.Information("--- Dump Data Update completed ---");
        }
    }
}