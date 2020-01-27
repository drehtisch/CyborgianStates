using System;

namespace CyborgianStates
{
    public static class Program
    {
        static ILauncher Launcher = new Launcher();
        public static void Main() => Launcher.Run();
        public static void SetLauncher(ILauncher launcher)
        {
            Launcher = launcher;
        }
    }
}
