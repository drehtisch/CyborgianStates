using CyborgianStates.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IDataService
    {
        Task<object> ExecuteRequest(Request request);

        Task<bool> IsActionReady(RequestType requestType);

        Task WaitForAction(RequestType requestType, TimeSpan interval, CancellationToken cancellationToken);

        Task WaitForAction(RequestType requestType, TimeSpan interval);

        Task WaitForAction(RequestType requestType);
    }
}