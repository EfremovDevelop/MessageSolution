namespace WebSocketClient.Interfaces
{
    public interface IWebSocketClient
    {
        Task ConnectAsync();
        Task SendMessageAsync(string message);
    }
}