using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using CyborgianStates.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CyborgianStates.CommandHandling
{
    public class NationStatesApiRequestQueue : IRequestQueue
    {
        NationStatesApiDataService _dataService;
        ILogger _logger;
        public NationStatesApiRequestQueue(NationStatesApiDataService dataService)
        {
            _dataService = dataService;
            _logger = ApplicationLogging.CreateLogger(typeof(NationStatesApiRequestQueue));
        }
        private Queue<Request> requestQueue = new Queue<Request>();
        private bool isRunning = false;
        public Task Enqueue(Request request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            requestQueue.Enqueue(request);
            _logger.LogInformation($"Request '{request.Type}' has been queued. Queue Size: {requestQueue.Count}");
            Task.Run(() => Run());
            return Task.CompletedTask;
        }

        private async Task Run()
        {
            if (isRunning) return;
            while (requestQueue.Count > 0)
            {
                var type = requestQueue.Peek().Type;
                await _dataService.WaitForAction(type).ConfigureAwait(false);
                await ExecuteRequest(requestQueue.Dequeue()).ConfigureAwait(false);
            }
            isRunning = false;
        }

        private async Task ExecuteRequest(Request request)
        {
            try
            {
                switch (request.Type)
                {
                    case RequestType.GetBasicNationStats:
                        var eventId = Helpers.GetEventIdByType(LoggingEvent.GetNationStats);
                        var response = await _dataService.GetNationStatsAsync(request.Params["nationName"].ToString(), eventId).ConfigureAwait(false);
                        var xml = await response.ReadXml().ConfigureAwait(false);
                        request.Complete(xml);
                        break;
                    default:
                        request.Fail($"RequestType: {request.Type} not implemented.");
                        break;
                }
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, $"A error occured while executing request '{request.Type}'");
                request.Fail(e.Message);
            }
            catch (XmlException e)
            {
                _logger.LogError(e, $"A error occured while executing request '{request.Type}'");
                request.Fail("Couldn't process malformed response.");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"A error occured while executing request '{request.Type}'");
                request.Fail("Something went wrong. See logs for details.");
            }
        }
    }
}
