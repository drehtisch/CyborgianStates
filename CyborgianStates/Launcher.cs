using CyborgianStates.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace CyborgianStates
{
    public class Launcher : ILauncher
    {
        IBotService _botService = new BotService(Program.ServiceProvider.GetService<IMessageHandler>());
        public bool IsRunning { get; private set; }
        public async Task RunAsync()
        {   
            IsRunning = true;
            await _botService.RunAsync().ConfigureAwait(false);
        }

        public void SetBotService(IBotService botService)
        {
            _botService = botService;
        }
    }
}
