using System.Text.Json;

namespace TeamProject;

public class Authentication
{
    private const string StorageFileName = "storage.json";
    private readonly Dictionary<string, string> _storage = new();
    
    public Authentication()
    {
        if (File.Exists(StorageFileName))
        {
            using StreamReader streamReader = new(StorageFileName);
            var jsonString = streamReader.ReadToEnd();
            var storageAsList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonString);

            if (storageAsList != null)
            {
                foreach (var item in storageAsList)
                {
                    _storage[item["name"]] = item["password"];
                }
            }

            foreach (var user in _storage)
            {
                Console.WriteLine("User: " + user.Key + ", Password: " + user.Value);
            }
            
            return;
        }
        
        var fileStream = new FileStream(StorageFileName, FileMode.Create);
        fileStream.Close();
        File.WriteAllText(StorageFileName, "[]");
    }

    private void SaveToFile()
    {
        var storageAsList = _storage.Select(user => new Dictionary<string, string> { { "name", user.Key }, { "password", user.Value } }).ToList();
        var jsonString = JsonSerializer.Serialize(storageAsList, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(StorageFileName, jsonString);
        
    }

    public Response SignIn(string name, string password)
    {
        var isUserInStorage = _storage.TryGetValue(name, out var storagePassword);

        if (!isUserInStorage)
        {
            return new Response(false, "User does not exist!");
        }
        
        return storagePassword == password ? new Response(true, "Logged in!") : new Response(false, "Credentials mismatch!");
    }

    public Response SignUp(string name, string password)
    {
        var isUserInStorage = _storage.ContainsKey(name);

        if (isUserInStorage)
        {
            return new Response(false, "User already exists!");
        }
        
        _storage.Add(name, password);
        SaveToFile();
        return new Response(true, "Signed up!");
    }
}

public class Response(bool success, string message)
{
    public bool Success { get; set; } = success;
    public string Message { get; set; } = message;
}
