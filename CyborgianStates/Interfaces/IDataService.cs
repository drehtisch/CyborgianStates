using CyborgianStates.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IDataService
    {
        Task WaitForAction(RequestType requestType, TimeSpan interval, CancellationToken cancellationToken);
        Task WaitForAction(RequestType requestType, TimeSpan interval);
        Task<bool> IsActionReady(RequestType requestType);
    }
}
