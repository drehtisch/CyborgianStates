using CyborgianStates.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IDataService
    {
        Task WaitForAction(RequestType requestType, TimeSpan interval);
        Task WaitForAction(RequestType requestType);
        Task IsActionReady(RequestType requestType);
    }
}
