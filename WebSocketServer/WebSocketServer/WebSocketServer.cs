using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using WebSocketServer.Interfaces;

namespace WebSocketServer.WebSocketServer
{
    public class WebSocketServer : IWebSocketServer
    {
        private readonly HttpListener _listener;
        private readonly IMessageHandler _messageHandler;
        private readonly ConcurrentBag<WebSocket> _clients = new ConcurrentBag<WebSocket>();

        public WebSocketServer(string uri, IMessageHandler messageHandler)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(uri);
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("WebSocket server started...");
            ListenAsync();
        }

        public void Stop()
        {
            _listener.Stop();
            Console.WriteLine("WebSocket server stopped.");
        }

        private async void ListenAsync()
        {
            while (_listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = ProcessWebSocketRequest(context);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in WebSocket server: {ex.Message}");
                }
            }
        }

        private async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            var webSocketContext = await context.AcceptWebSocketAsync(null);
            var webSocket = webSocketContext.WebSocket;
            _clients.Add(webSocket);

            var buffer = new byte[1024];
            Console.WriteLine("New client connected.");

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Client disconnected.");
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                        break;
                    }

                    // Broadcast message to all clients
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var processedMessage = await _messageHandler.HandleMessageAsync(message);
                    await BroadcastMessageAsync(processedMessage);
                }
            }
            finally
            {
                _clients.TryTake(out _);
            }
        }
        private async Task BroadcastMessageAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);

            foreach (var client in _clients)
            {
                if (client.State == WebSocketState.Open)
                {
                    try
                    {
                        await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send message to a client: {ex.Message}");
                    }
                }
            }
        }
    }
}
