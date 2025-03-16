using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebSocket_Server;

public class TestService : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        Console.WriteLine("Received from client: " + e.Data);
        
        Send("Data from server");
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        // do nothing
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var ws = new WebSocketServer("ws://localhost:8081");

        ws.AddWebSocketService<TestService>("/test");
        ws.Start();
        Console.ReadKey(true);
        ws.Stop();      
    }
}
