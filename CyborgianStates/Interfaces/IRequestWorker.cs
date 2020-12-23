using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IRequestWorker
    {
        void Enqueue(Request request, int priority);
        Task RunAsync(CancellationToken cancellationToken);
    }
}