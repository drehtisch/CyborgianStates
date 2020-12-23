using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.CommandHandling
{
    public class NationStatesApiRequestWorker : IRequestWorker
    {
        private readonly IDataService _dataService;
        private readonly ILogger _logger;

        private readonly RequestPriorityQueue _requestQueue = new();
        public NationStatesApiRequestWorker(IDataService dataService)
        {
            _dataService = dataService;
            _logger = ApplicationLogging.CreateLogger(typeof(NationStatesApiRequestWorker));
        }


        public void Enqueue(Request request, int priority = 1000)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            _requestQueue.Enqueue(request, priority);
            _logger.LogDebug($"[{request.TraceId}]: Request '{request.Type}' has been queued. Queue Size: {_requestQueue.Count}");
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (await _requestQueue.WaitForNextItemAsync(cancellationToken).ConfigureAwait(false))
            {
                var request = _requestQueue.Dequeue();
                _logger.LogDebug($"[{request.TraceId}]: Request '{request.Type}' has been dequeued. Queue Size: {_requestQueue.Count}");
                await _dataService.ExecuteRequestAsync(request).ConfigureAwait(false);
            }
        }
    }
}