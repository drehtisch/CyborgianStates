using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CyborgianStates.CommandHandling
{
    public class NationStatesApiRequestQueue : IRequestQueue
    {
        private readonly IDataService _dataService;
        private readonly ILogger _logger;

        private readonly Queue<Request> _requestQueue = new Queue<Request>();

        private bool isRunning = false;

        public NationStatesApiRequestQueue(IDataService dataService)
        {
            _dataService = dataService;
            _logger = ApplicationLogging.CreateLogger(typeof(NationStatesApiRequestQueue));
        }

        public int Size => _requestQueue.Count;

        public async Task<int> Enqueue(Request request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            _requestQueue.Enqueue(request);
            var position = Size;
            _logger.LogDebug($"Request '{request.Type}' has been queued. Queue Size: {_requestQueue.Count}");
            _ = RunAsync();
            return await Task.FromResult(position).ConfigureAwait(false);
        }

        private async Task ExecuteRequestAsync(Request request)
        {
            try
            {
                if (await _dataService.ExecuteRequest(request).ConfigureAwait(false) is HttpResponseMessage response)
                {
                    var xml = await response.ReadXmlAsync().ConfigureAwait(false);
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

        private async Task RunAsync()
        {
            if (isRunning)
                return;
            while (_requestQueue.Count > 0)
            {
                isRunning = true;
                var type = _requestQueue.Peek().Type;
                await _dataService.WaitForAction(type).ConfigureAwait(false);
                await ExecuteRequestAsync(_requestQueue.Dequeue()).ConfigureAwait(false);
            }
            isRunning = false;
        }
    }
}