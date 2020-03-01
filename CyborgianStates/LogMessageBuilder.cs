using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates
{
    public static class LogMessageBuilder
    {
        public static string Build(EventId eventId, string message)
        {
            return $"[{eventId.Id}] {message}";
        }
    }
}
