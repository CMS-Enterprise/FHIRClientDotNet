using System.Text.Json;
using System.Text.Json.Nodes;


namespace FHIRClientUtilities
{
    public static class JsonUtil
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Serialize an object to JSON string
        public static string ToJson<T>(T obj, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize(obj, options ?? DefaultOptions);
        }

        // Deserialize a JSON string to an object
        public static T? FromJson<T>(string json, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }

        public static JsonNode? ParseJsonToDynamic(string jsonString)
        {
            try
            {
                return JsonNode.Parse(jsonString);
            }
            catch
            {
                // You could log or handle the error here if needed
                return null;
            }
        }

        public static async Task<T?> LoadFromJsonFileAsync<T>(string filePath, JsonSerializerOptions? options = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("JSON file not found.", filePath);

            string jsonString = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(jsonString,options ?? DefaultOptions);
        }
        public static async Task<List<T>> LoadFromJsonFolderAsync<T>(string folderPath, string searchPattern = "*.json", JsonSerializerOptions? options = null)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

            var files = Directory.GetFiles(folderPath, searchPattern);
            var result = new List<T>();
            

            foreach (var file in files)
            {
                try
                {
                    string json = await File.ReadAllTextAsync(file);
                    T? obj = JsonSerializer.Deserialize<T>(json, options??DefaultOptions);
                    if (obj != null)
                        result.Add(obj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading file '{file}': {ex.Message}");
                    // Optionally log or continue silently
                }
            }

            return result;
        }

    }

}




