using CyborgianStates.Enums;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IRequestDispatcher
    {
        Task Dispatch(Request request);

        Task Register(DataSourceType dataSource, IRequestQueue requestQueue);
    }
}