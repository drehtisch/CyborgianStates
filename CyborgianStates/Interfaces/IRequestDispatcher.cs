using CyborgianStates.Enums;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IRequestDispatcher
    {
        void Dispatch(Request request, int priority);
        void Register(DataSourceType dataSource, IRequestWorker requestQueue);
        void Start();
        void Shutdown();
    }
}