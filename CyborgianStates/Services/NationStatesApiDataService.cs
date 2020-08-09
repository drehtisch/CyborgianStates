using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Services
{
    public class NationStatesApiDataService : IDataService
    {
        public const long API_REQUEST_INTERVAL = 6500000;
        public const int API_VERSION = 10;

        //0,6 s + additional 0,05 s as buffer -> 0,65 s
        public const long REQUEST_NEW_NATIONS_INTERVAL = 18000000000; //30 m 18000000000

        public const long REQUEST_REGION_NATIONS_INTERVAL = 432000000000; //12 h 432000000000
        public const long SEND_RECRUITMENTTELEGRAM_INTERVAL = 1800000000; //3 m 1800000000

        private readonly IHttpDataService _dataService;
        private readonly ILogger _logger;

        private DateTime lastAPIRequest;

        public NationStatesApiDataService(IHttpDataService dataService)
        {
            _dataService = dataService;
            _logger = ApplicationLogging.CreateLogger(typeof(NationStatesApiDataService));
        }

        public static Uri BuildApiRequestUrl(string parameters)
        {
            return new Uri($"http://www.nationstates.net/cgi-bin/api.cgi?{parameters}&v={API_VERSION}");
        }

        public async Task<object> ExecuteRequest(Request request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            switch (request.Type)
            {
                case RequestType.GetBasicNationStats:
                    return await GetNationStatsAsync(request.Params["nationName"].ToString(), request.EventId).ConfigureAwait(false);

                case RequestType.UnitTest:
                default:
                    throw new InvalidOperationException($"Unknown request type: {request.Type}");
            }
        }

        public Task<bool> IsActionReady(RequestType requestType)
        {
            if (requestType == RequestType.GetBasicNationStats)
            {
                return Task.FromResult(DateTime.UtcNow.Ticks - lastAPIRequest.Ticks > API_REQUEST_INTERVAL);
            }
            else
            {
                _logger.LogCritical($"Unrecognized RequestType '{requestType}'");
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

        private async Task<HttpResponseMessage> GetNationStatsAsync(string nationName, EventId eventId)
        {
            _logger.LogDebug(eventId, LogMessageBuilder.Build(eventId, $"Waiting for NationStats-Request: {nationName}"));
            await WaitForAction(RequestType.GetBasicNationStats).ConfigureAwait(false);
            Uri url = BuildApiRequestUrl($"nation={Helpers.ToID(nationName)}&q=flag+wa+gavote+scvote+fullname+freedom+demonym2plural+category+population+region+founded+influence+lastactivity+census;mode=score;scale=0+1+2+65+66+80");
            var message = new HttpRequestMessage(HttpMethod.Get, url);
            lastAPIRequest = DateTime.UtcNow;
            try
            {
                return await _dataService.ExecuteRequest(message, eventId).ConfigureAwait(false);
            }
            finally
            {
                message.Dispose();
            }
        }
    }
}