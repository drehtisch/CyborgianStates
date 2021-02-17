using CyborgianStates.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates
{
    public class Request
    {
        public Request(RequestType type, ResponseFormat format, DataSourceType dataSource)
        {
            Type = type;
            ExpectedReponseFormat = format;
            DataSourceType = dataSource;
            EventId = Helpers.GetEventIdByRequestType(type);
            _completionSource = new TaskCompletionSource<object>();
            TraceId = GenerateTraceId();
        }

        public Request()
        {
        }

        private readonly TaskCompletionSource<object> _completionSource;
        public DataSourceType DataSourceType { get; }
        public EventId EventId { get; private set; }
        public string TraceId { get; private set; }
        public ResponseFormat ExpectedReponseFormat { get; }
        public string FailureReason { get; private set; }
        public Dictionary<string, object> Params { get; } = new Dictionary<string, object>();
        public int Priority { get; set; }
        public object Response { get; private set; }
        public RequestStatus Status { get; private set; }
        public RequestType Type { get; }

        public void Complete(object response)
        {
            Response = response;
            Status = RequestStatus.Success;
            _completionSource.TrySetResult(response);
        }

        public void Fail(string failReason, Exception ex)
        {
            FailureReason = failReason;
            Status = RequestStatus.Failed;
            if (ex is null)
            {
                ex = new Exception($"Error not specified. FailureReason: {FailureReason}");
            }

            _completionSource.TrySetException(ex);
        }

        public Task<object> WaitForResponseAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                Status = RequestStatus.Canceled;
                _completionSource.TrySetCanceled(cancellationToken);
            });
            return _completionSource.Task;
        }

        private string GenerateTraceId()
        {
            StringBuilder builder = new StringBuilder();
            Enumerable
               .Range(65, 26)
                .Select(e => ((char) e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char) e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(11)
                .ToList().ForEach(e => builder.Append(e));
            return builder.ToString();
        }
    }
}