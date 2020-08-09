using CyborgianStates.CommandHandling;
using CyborgianStates.MessageHandling;
using System.Threading;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface ICommand
    {
        Task<CommandResponse> Execute(Message message);

        void SetCancellationToken(CancellationToken cancellationToken);
    }
}