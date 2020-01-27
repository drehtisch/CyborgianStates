using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.CommandHandling
{
    public class CommandResponse
    {
        public CommandResponse(CommandStatus status, string content)
        {
            Status = status;
            Content = content;
        }
        public CommandStatus Status { get; }
        public string Content { get; }
    }

    public enum CommandStatus
    {
        Success,
        Error
    }
}
