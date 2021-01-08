using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.CommandHandling
{
    public class RequestDispatcher : IRequestDispatcher
    {
        private readonly Dictionary<DataSourceType, IRequestWorker> _workers = new();
        private bool _isRunning = false;
        private readonly CancellationTokenSource _tokenSource = new();
        public void Dispatch(Request request, int priority)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (_workers.ContainsKey(request.DataSourceType))
            {
                var queue = _workers[request.DataSourceType];
                queue.Enqueue(request, priority);
            }
            else
            {
                throw new InvalidOperationException($"Unable to dispatch this request to any queue. No queue of dataSourceType: {request.DataSourceType} registered.");
            }
        }

        public void Register(DataSourceType dataSource, IRequestWorker requestQueue)
        {
            if (!_isRunning)
            {
                _workers[dataSource] = requestQueue;
                requestQueue.RestartRequired += RequestQueue_RestartRequired;
            }
            else
            {
                throw new InvalidOperationException("RequestWorkers can't be registered when the dispatcher already has been started.");
            }
        }

        private void RequestQueue_RestartRequired(object sender, EventArgs e)
        {
            if(sender is IRequestWorker worker)
            {
                Task.Run(async () => await worker.RunAsync(_tokenSource.Token).ConfigureAwait(false));
            }
        }

        public void Shutdown()
        {
            _tokenSource.Cancel();
            foreach (var worker in _workers)
            {
                worker.Value.RestartRequired -= RequestQueue_RestartRequired;
            }
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                foreach (var worker in _workers)
                {
                    Task.Run(async () => await worker.Value.RunAsync(_tokenSource.Token).ConfigureAwait(false));
                }
            }
            else
            {
                throw new InvalidOperationException("The dispatcher is already running.");
            }
        }
    }
}