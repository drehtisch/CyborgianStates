using CyborgianStates.CommandHandling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IMessageChannel
    {
        bool IsPrivate { get; }
        Task WriteToAsync(bool isPublic, string message);
        Task WriteToAsync(bool isPublic, CommandResponse response);
    }
}
