using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates
{
    public class Launcher : ILauncher
    {
        public bool IsRunning { get; private set; }
        public void Run()
        {
            IsRunning = true;
        }
    }

    public interface ILauncher
    {
        bool IsRunning { get; }
        void Run();
    }
}
