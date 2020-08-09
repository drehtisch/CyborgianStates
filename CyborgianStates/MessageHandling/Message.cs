using CyborgianStates.Interfaces;

namespace CyborgianStates.MessageHandling
{
    public class Message
    {
        public Message(ulong authorId, string content, IMessageChannel channel)
        {
            AuthorId = authorId;
            Content = content;
            Channel = channel;
        }

        public ulong AuthorId { get; }
        public IMessageChannel Channel { get; }
        public string Content { get; }
    }
}