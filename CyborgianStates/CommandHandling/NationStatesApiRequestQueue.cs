using CyborgianStates.Interfaces;
using CyborgianStates.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.CommandHandling
{
    public class NationStatesApiRequestQueue : IRequestQueue
    {
        NationStatesApiDataService _dataService;
        public NationStatesApiRequestQueue(NationStatesApiDataService dataService)
        {
            _dataService = dataService;
        }
        private Queue<Request> requestQueue = new Queue<Request>();
        private bool isRunning = false;
        public Task Enqueue(Request request)
        {
            requestQueue.Enqueue(request);
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
                //ExecuteRequest
            }
            isRunning = false;
        }
    }
}
