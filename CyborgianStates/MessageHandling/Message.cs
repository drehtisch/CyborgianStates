using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.MessageHandling
{
    public class Message
    {
        public Message (ulong authorId, string content, IMessageChannel channel)
        {
            AuthorId = authorId;
            Content = content;
            Channel = channel;
        }
        public ulong AuthorId { get; }
        public string Content { get; }
        public IMessageChannel Channel {get; }
}
}
