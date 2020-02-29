using CyborgianStates.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IRequestDispatcher
    {
        Task Register(DataSourceType dataSource, IRequestQueue requestQueue);
        Task Dispatch(IRequest request);
    }
}
