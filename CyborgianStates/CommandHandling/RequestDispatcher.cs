using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyborgianStates.CommandHandling
{
    public class RequestDispatcher : IRequestDispatcher
    {
        private readonly Dictionary<DataSourceType, IRequestQueue> Queues = new Dictionary<DataSourceType, IRequestQueue>();

        public async Task Dispatch(Request request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            if (Queues.ContainsKey(request.DataSourceType))
            {
                var queue = Queues[request.DataSourceType];
                await queue.Enqueue(request).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException($"Unable to dispatch this request to any queue. No queue of dataSourceType: {request.DataSourceType} registered.");
            }
        }

        public Task Register(DataSourceType dataSource, IRequestQueue requestQueue)
        {
            Queues[dataSource] = requestQueue;
            return Task.CompletedTask;
        }
    }
}