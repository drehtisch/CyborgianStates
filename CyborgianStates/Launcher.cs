using CyborgianStates.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace CyborgianStates
{
    public class Launcher : ILauncher
    {
        private IBotService _botService;
        public bool IsRunning { get; private set; }

        public async Task RunAsync()
        {
            IsRunning = true;
            _botService = Program.ServiceProvider.GetService<IBotService>();
            await _botService.InitAsync().ConfigureAwait(false);
            await _botService.RunAsync().ConfigureAwait(false);
        }
    }
}