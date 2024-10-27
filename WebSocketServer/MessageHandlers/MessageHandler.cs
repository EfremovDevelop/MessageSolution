using WebSocketServer.Interfaces;

namespace WebSocketServer.MessageHandlers
{
    public class MessageHandler : IMessageHandler
    {
        public Task<string> HandleMessageAsync(string message)
        {
            var timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            return Task.FromResult(timestampedMessage);
        }
    }
}
