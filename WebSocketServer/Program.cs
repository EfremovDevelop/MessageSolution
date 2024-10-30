using Microsoft.Extensions.DependencyInjection;
using WebSocketServer.MessageHandlers;

var serviceProvider = new ServiceCollection()
            .AddSingleton<MessageHandler>()
            .AddSingleton<WebSocketServer.WebSocketServer.WebSocketServer>(provider =>
            {
                var (host, port) = ReadConnection();
                return new WebSocketServer.WebSocketServer.WebSocketServer($"http://{host}:{port}/", provider.GetRequiredService<MessageHandler>());
            })
            .BuildServiceProvider();

var server = serviceProvider.GetRequiredService<WebSocketServer.WebSocketServer.WebSocketServer>();
server.Start();

Console.WriteLine("Press Enter to stop the server...");
Console.ReadLine();

server.Stop();

static (string host, int port) ReadConnection()
{
    Console.Write("Host: ");
    var host = Console.ReadLine();
    Console.Write("Port: ");
    var port = int.Parse(Console.ReadLine());
    return (host, port);
}