using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IRequestQueue
    {
        int Size { get; }

        Task<int> Enqueue(Request request);
    }
}