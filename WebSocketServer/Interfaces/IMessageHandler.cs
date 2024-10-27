namespace WebSocketServer.Interfaces
{
    public interface IMessageHandler
    {
        Task<string> HandleMessageAsync(string message);
    }
}
