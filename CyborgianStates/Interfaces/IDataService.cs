using CyborgianStates.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IDataService
    {
        Task ExecuteRequestAsync(Request request);
    }
}