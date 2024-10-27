(string host, int port) = ReadConnection();

string serverUri = $"ws://{host}:{port}/";
var client = new WebSocketClient.WebSocketClient.WebSocketClient(serverUri);

await client.ConnectAsync();

Console.WriteLine("Enter messages to send to the server. Type 'exit' to quit.");

while (true)
{
    var message = Console.ReadLine();
    if (message?.ToLower() == "exit")
    {
        break;
    }

    await client.SendMessageAsync(message);
}

Console.WriteLine("Client stopped.");

static (string host, int port) ReadConnection()
{
    Console.Write("Host: ");
    var host = Console.ReadLine();
    Console.Write("Port: ");
    var port = int.Parse(Console.ReadLine());
    return (host, port);
}