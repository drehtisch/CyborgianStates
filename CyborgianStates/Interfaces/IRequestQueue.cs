using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IRequestQueue
    {
        int Size { get; }
        Task Enqueue(IRequest request);
    }
}
