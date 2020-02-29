using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.CommandHandling
{
    public class RequestDispatcher : IRequestDispatcher
    {
        readonly Dictionary<DataSourceType, IRequestQueue> Queues = new Dictionary<DataSourceType, IRequestQueue>();
        public async Task Dispatch(IRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
                
            if (Queues.ContainsKey(request.DataSourceType))
            {
                var queue = Queues[request.DataSourceType];
                await queue.Enqueue(request).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException($"Unable to dispatch this request to a queue. No queue of dataSourceType: {request.DataSourceType} registered.");
            }
        }

        public Task Register(DataSourceType dataSource, IRequestQueue requestQueue)
        {
            Queues[dataSource] = requestQueue;
            return Task.CompletedTask;
        }
    }
}
