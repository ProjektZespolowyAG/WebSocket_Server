using System.Text.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TeamProject;

public class ClientMessage
{
    public string Type { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Content { get; set; } = "";
}

public class ServerResponse
{
    public string Type { get; set; } = "";
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}

public class ChatService : WebSocketBehavior
{
    private static readonly Authentication Auth = new();
    private string? _currentUser;

    protected override void OnMessage(MessageEventArgs e)
    {
        try
        {
            Console.WriteLine($"Received: {e.Data}");
            
            var message = JsonSerializer.Deserialize<ClientMessage>(e.Data);
            
            if (message == null)
            {
                SendResponse("error", false, "Invalid message format");
                return;
            }

            switch (message.Type.ToLower())
            {
                case "signin":
                    HandleSignIn(message.Username, message.Password);
                    break;
                case "signup":
                    HandleSignUp(message.Username, message.Password);
                    break;
                case "message":
                    HandleMessage(message.Content);
                    break;
                default:
                    Send("Data from server: " + e.Data);
                    break;
            }
        }
        catch (JsonException)
        {
            Console.WriteLine("Received non-JSON message: " + e.Data);
            Send("Data from server: " + e.Data);
        }
    }

    private void HandleSignIn(string username, string password)
    {
        var response = Auth.SignIn(username, password);
        
        if (response.Success)
        {
            _currentUser = username;
            Console.WriteLine($"User {username} signed in");
        }
        
        SendResponse("signin", response.Success, response.Message);
    }

    private void HandleSignUp(string username, string password)
    {
        var response = Auth.SignUp(username, password);
        
        if (response.Success)
        {
            Console.WriteLine($"New user {username} registered");
        }
        
        SendResponse("signup", response.Success, response.Message);
    }

    private void HandleMessage(string content)
    {
        if (_currentUser == null)
        {
            SendResponse("error", false, "Must be logged in to send messages");
            return;
        }

        var broadcastMessage = $"{_currentUser}: {content}";
        Sessions.Broadcast(broadcastMessage);
        
        Console.WriteLine($"[{_currentUser}]: {content}");
    }

    private void SendResponse(string type, bool success, string message)
    {
        var response = new ServerResponse
        {
            Type = type,
            Success = success,
            Message = message
        };
        
        Send(JsonSerializer.Serialize(response));
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Console.WriteLine($"WebSocket Error: {e.Message}");
    }

    protected override void OnClose(CloseEventArgs e)
    {
        if (_currentUser != null)
        {
            Console.WriteLine($"User {_currentUser} disconnected");
            _currentUser = null;
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var ws = new WebSocketServer("ws://localhost:8081");

        ws.AddWebSocketService<ChatService>("/chat");
        
        ws.AddWebSocketService<TestService>("/test");
        
        ws.Start();
        Console.WriteLine("WebSocket Server started on ws://localhost:8081");
        Console.WriteLine("Available endpoints:");
        Console.WriteLine("  /chat - Chat service with authentication");
        Console.WriteLine("  /test - Original test service");
        Console.WriteLine("Press any key to stop server...");
        
        Console.ReadKey(true);
        ws.Stop();
        
        Console.WriteLine("Server stopped.");
    }
}

public class TestService : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        Console.WriteLine("Received from client: " + e.Data);
        Send("Data from server");
    }

    protected override void OnError(ErrorEventArgs e)
    {
        // do nothing romek
    }
}
