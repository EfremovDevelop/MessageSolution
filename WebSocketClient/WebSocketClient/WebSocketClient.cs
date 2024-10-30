using System.Net.WebSockets;
using System.Text;
using WebSocketClient.Interfaces;

namespace WebSocketClient.WebSocketClient
{
    public class WebSocketClient : IWebSocketClient
    {
        private readonly string _serverUri;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private const int PingInterval = 10000;

        public WebSocketClient(string serverUri)
        {
            _serverUri = serverUri;
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync()
        {
            while (_webSocket.State != WebSocketState.Open)
            {
                try
                {
                    Console.WriteLine("Attempting to connect to the server...");
                    await _webSocket.ConnectAsync(new Uri(_serverUri), CancellationToken.None);
                    Console.WriteLine("Connected to the server.");
                    _ = StartPingAsync();
                    _ = ReceiveMessagesAsync();
                }
                catch (Exception ex)
                {
                    _webSocket = new ClientWebSocket();
                    Console.WriteLine($"Connection failed: {ex.Message}. Retrying in 5 seconds...");
                    await Task.Delay(5000);
                }
            }
        }

        private async Task StartPingAsync()
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await _webSocket.SendAsync(ArraySegment<byte>.Empty, WebSocketMessageType.Text, true, CancellationToken.None);
                    await Task.Delay(PingInterval, _cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ping failed: {ex.Message}");
                    await ReconnectAsync();
                    break;
                }
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024];

            while (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Server closed the connection.");
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by server", CancellationToken.None);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (message == string.Empty)
                        Console.WriteLine($"ping: {message}");
                    else
                        Console.WriteLine($"Message from server: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                    await ReconnectAsync();
                    break;
                }
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                Console.WriteLine("Cannot send message. Not connected to server.");
            }
        }

        private async Task ReconnectAsync()
        {
            Console.WriteLine("Reconnecting to server...");
            _cancellationTokenSource.Cancel();
            _webSocket.Dispose();
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
            await ConnectAsync();
        }
    }
}
