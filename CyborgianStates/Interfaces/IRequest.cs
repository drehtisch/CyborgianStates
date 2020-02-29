using CyborgianStates.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IRequest
    {
        RequestStatus Status { get; }
        RequestType Type { get; }
        Dictionary<string, object> Params { get; }
        object Response { get; set; }
        ResponseFormat ExpectedReponseFormat { get; }
        int Priority { get; }
        DataSourceType DataSourceType { get; }
        string FailureReason { get; set; }
        Task WaitForResponse();
        Task Cancel();
    }
}
