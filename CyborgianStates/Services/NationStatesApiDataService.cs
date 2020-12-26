using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Services
{
    public class NationStatesApiDataService : IDataService
    {
        public const long API_REQUEST_INTERVAL = 6125000; //0,6 s + additional 0,0125 s as buffer -> 0,6125 s
        public const int API_VERSION = 10;

        private readonly IHttpDataService _dataService;
        private readonly ILogger _logger;

        //private DateTime lastAPIRequest;
        private Dictionary<string, SemaphoreSlim> _semaphores = new();
        public NationStatesApiDataService(IHttpDataService dataService)
        {
            _dataService = dataService;
            _logger = ApplicationLogging.CreateLogger(typeof(NationStatesApiDataService));
            _semaphores["API_REQUEST"] = new SemaphoreSlim(1, 1);
        }

        public static Uri BuildApiRequestUrl(string parameters)
        {
            return new Uri($"http://www.nationstates.net/cgi-bin/api.cgi?{parameters}&v={API_VERSION}");
        }

        public async Task ExecuteRequestAsync(Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var semaphore = GetSemaphoreByRequestType(request.Type);
            var ticks = DateTime.UtcNow.Ticks;
            _logger.LogDebug($"[{request.TraceId}]: Acquiring Semaphore");
            await semaphore.WaitAsync().ConfigureAwait(false);
            _logger.LogDebug($"[{request.TraceId}]: Aquiring Semaphore took {TimeSpan.FromTicks(DateTime.UtcNow.Ticks).Subtract(TimeSpan.FromTicks(ticks))}");
            HttpResponseMessage result = null;
            Action<Task<HttpResponseMessage>> _continue = async (prev) =>
            {
                await Task.Delay(TimeSpan.FromTicks(GetTimeoutByRequestType(request.Type))).ConfigureAwait(false);
                if (semaphore.CurrentCount == 0)
                {
                    semaphore.Release();
                }
            };
            try
            {
                switch (request.Type)
                {
                    case RequestType.GetBasicNationStats:
                        var task = GetNationStatsAsync(request.Params["nationName"].ToString(), request.EventId);
                        _ = task.ContinueWith(_continue);
                        result = await task.ConfigureAwait(false);
                        break;
                }
                if (result is not null && result.IsSuccessStatusCode)
                {
                    var xml = await result.ReadXmlAsync().ConfigureAwait(false);
                    request.Complete(xml);
                }
                else if (result is null)
                {
                    var reason = "Request failed: No result";
                    request.Fail(reason, new InvalidOperationException(reason));
                }
                else
                {
                    var reason = $"Request failed: {(int) result.StatusCode} - {result.StatusCode}";
                    request.Fail(reason, new HttpRequestFailedException(reason));
                }
            }
            finally
            {
                result?.Dispose();
            }
        }

        private SemaphoreSlim GetSemaphoreByRequestType(RequestType requestType)
        {
            switch (requestType)
            {
                case RequestType.GetBasicNationStats:
                    return _semaphores["API_REQUEST"];
                default:
                    throw new InvalidOperationException($"Unrecognized RequestType '{requestType}'");
            }
        }

        private long GetTimeoutByRequestType(RequestType requestType)
        {
            switch (requestType)
            {
                case RequestType.GetBasicNationStats:
                    return API_REQUEST_INTERVAL;
                default:
                    throw new InvalidOperationException($"Unrecognized RequestType '{requestType}'");
            }
        }

        private async Task<HttpResponseMessage> GetNationStatsAsync(string nationName, EventId eventId)
        {
            Uri url = BuildApiRequestUrl($"nation={Helpers.ToID(nationName)}&q=flag+wa+gavote+scvote+fullname+freedom+demonym2plural+category+population+region+founded+influence+lastactivity+census;mode=score;scale=0+1+2+65+66+80");
            var message = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                return await _dataService.ExecuteRequestAsync(message, eventId).ConfigureAwait(false);
            }
            finally
            {
                message.Dispose();
            }
        }
    }

    public class HttpRequestFailedException : Exception
    {
        public HttpRequestFailedException() { }

        public HttpRequestFailedException(string message) : base(message) { }

        public HttpRequestFailedException(string message, Exception innerException) : base(message, innerException) { }

        protected HttpRequestFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}