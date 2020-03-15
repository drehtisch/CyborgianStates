using CyborgianStates.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public class Request
    {
        public Request(RequestType type, ResponseFormat format, DataSourceType dataSource)
        {
            Type = type;
            ExpectedReponseFormat = format;
            DataSourceType = dataSource;
        }
        public RequestStatus Status { get; private set; }
        public RequestType Type { get; }
        public Dictionary<string, object> Params { get; } = new Dictionary<string, object>();
        public object Response { get; private set; }
        public ResponseFormat ExpectedReponseFormat { get; }
        public int Priority { get; set; }
        public DataSourceType DataSourceType { get; }
        public string FailureReason { get; private set; }
        public void Complete(object response)
        {
            Response = response;
            Status = RequestStatus.Success;
        }
        public void Fail(string failReason)
        {
            FailureReason = failReason;
            Status = RequestStatus.Failed;
        }
        public async Task WaitForResponse(CancellationToken cancellationToken)
        {
            while (Status == RequestStatus.Pending)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Status = RequestStatus.Canceled;
                }
                else
                {
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }
        }
    }
}
