using WebSocketServer.MessageHandlers;

(string host, int port) = ReadConnection();

var messageHandler = new MessageHandler();
var server = new WebSocketServer.WebSocketServer.WebSocketServer($"http://{host}:{port}/", messageHandler);

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