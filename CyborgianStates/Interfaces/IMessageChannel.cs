using CyborgianStates.CommandHandling;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IMessageChannel
    {
        bool IsPrivate { get; }

        Task WriteToAsync(bool isPublic, CommandResponse response);
    }
}