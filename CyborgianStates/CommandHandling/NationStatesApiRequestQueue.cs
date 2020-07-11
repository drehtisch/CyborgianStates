using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using CyborgianStates.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CyborgianStates.CommandHandling
{
    public class NationStatesApiRequestQueue : IRequestQueue
    {
        IDataService _dataService;
        ILogger _logger;
        public NationStatesApiRequestQueue(IDataService dataService)
        {
            _dataService = dataService;
            _logger = ApplicationLogging.CreateLogger(typeof(NationStatesApiRequestQueue));
        }
        private Queue<Request> requestQueue = new Queue<Request>();
        private bool isRunning = false;

        public int Size => requestQueue.Count;

        public async Task<int> Enqueue(Request request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            requestQueue.Enqueue(request);
            var position = Size;
            _logger.LogInformation($"Request '{request.Type}' has been queued. Queue Size: {requestQueue.Count}");
            _ = Run();
            return await Task.FromResult(position).ConfigureAwait(false);
        }

        private async Task Run()
        {
            if (isRunning) return;
            while (requestQueue.Count > 0)
            {
                isRunning = true;
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
                if (await _dataService.ExecuteRequest(request).ConfigureAwait(false) is HttpResponseMessage response)
                {
                    var xml = await response.ReadXml().ConfigureAwait(false);
                    request.Complete(xml);
                    response.Dispose();
                }
                else
                {
                    throw new InvalidOperationException("Response of DataService was not of type HttpResponseMessage");
                }
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, $"A error occured while executing request '{request.Type}'");
                request.Fail(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"A error occured while executing request '{request.Type}'");
                request.Fail("Something went wrong. See logs for details.");
            }
        }
    }
}
