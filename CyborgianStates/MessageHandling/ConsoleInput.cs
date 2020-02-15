using CyborgianStates.Interfaces;
using System;

namespace CyborgianStates.MessageHandling
{
    public class ConsoleInput : IUserInput
    {
        public string GetInput()
        {
            if (Console.KeyAvailable)
            {
                Console.Write("> ");
                return Console.ReadLine().Trim();
            }
            else
            {
                return string.Empty;
            }
        }
    }

    //// Unit Test
    //public class FakeUserInput : IUserInput
    //{
    //    public string GetInput()
    //    {
    //        return "ABC_123";
    //    }
    //}
}
