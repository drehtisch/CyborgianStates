using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CyborgianStates.Services
{
    public class NationStatesApiDataService : IDataService
    {
        public const int API_VERSION = 10;
        public const long API_REQUEST_INTERVAL = 6500000; //0,6 s + additional 0,05 s as buffer -> 0,65 s
        public const long REQUEST_NEW_NATIONS_INTERVAL = 18000000000; //30 m 18000000000
        public const long REQUEST_REGION_NATIONS_INTERVAL = 432000000000; //12 h 432000000000
        public const long SEND_RECRUITMENTTELEGRAM_INTERVAL = 1800000000; //3 m 1800000000

        IHttpDataService _dataService;
        ILogger<NationStatesApiDataService> _logger;

        DateTime LastAPIRequest;
        //DateTime LastTelegramSending;
        //DateTime LastNewNationsRequest;

        public NationStatesApiDataService(IHttpDataService dataService, ILogger<NationStatesApiDataService> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }
        public Task<bool> IsActionReady(RequestType requestType)
        {
            if (requestType == RequestType.GetBasicNationStats)
            {
                return Task.FromResult(DateTime.UtcNow.Ticks - LastAPIRequest.Ticks > API_REQUEST_INTERVAL);
            }
            else
            {
                _logger.LogCritical($"Unrecognized RequestType '{requestType.ToString()}'");
                return Task.FromResult(false);
            }
        }
        public async Task WaitForAction(RequestType requestType, TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!await IsActionReady(requestType).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                await Task.Delay(interval.Milliseconds, cancellationToken).ConfigureAwait(false);
            }
        }
        public async Task WaitForAction(RequestType requestType, TimeSpan interval)
        {
            while (!await IsActionReady(requestType).ConfigureAwait(false))
            {
                await Task.Delay(interval.Milliseconds).ConfigureAwait(false);
            }
        }
        public async Task WaitForAction(RequestType requestType)
        {
            await WaitForAction(requestType, TimeSpan.FromTicks(API_REQUEST_INTERVAL)).ConfigureAwait(false);
        }
        public async Task<XmlDocument> GetNationStatsAsync(string nationName, EventId eventId)
        {
            _logger.LogDebug(eventId, LogMessageBuilder.Build(eventId, $"Waiting for NationStats-Request: {nationName}"));
            await WaitForAction(RequestType.GetBasicNationStats).ConfigureAwait(false);
            Uri url = BuildApiRequestUrl($"nation={Helpers.ToID(nationName)}&q=flag+wa+gavote+scvote+fullname+freedom+demonym2plural+category+population+region+founded+influence+lastactivity+census;mode=score;scale=0+1+2+65+66+80");
            var message = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                return await _dataService.ExecuteRequestWithXmlResult(message, eventId).ConfigureAwait(false);
            }
            finally
            {
                message.Dispose();
            }
        }
        private static Uri BuildApiRequestUrl(string parameters)
        {
            return new Uri($"http://www.nationstates.net/cgi-bin/api.cgi?{parameters}&v={API_VERSION}");
        }
    }
}
