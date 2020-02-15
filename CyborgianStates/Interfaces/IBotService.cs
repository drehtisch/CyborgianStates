using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IBotService
    {
        bool IsRunning { get; }
        Task InitAsync();
        Task RunAsync();
        Task ShutdownAsync();
    }
}