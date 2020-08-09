namespace CyborgianStates.CommandHandling
{
    public enum CommandStatus
    {
        Success,
        Error
    }

    public class CommandResponse
    {
        public CommandResponse(CommandStatus status, string content)
        {
            Status = status;
            Content = content;
        }

        public string Content { get; }
        public CommandStatus Status { get; }
    }
}