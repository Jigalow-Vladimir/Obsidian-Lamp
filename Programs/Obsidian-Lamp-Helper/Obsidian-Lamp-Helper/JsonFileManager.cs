using System.IO;
using System.Text;
using System.Text.Json;

namespace Obsidian_Lamp_Helper
{
    public class JsonFileManager
    {
        private readonly JsonSerializerOptions _options;

        public JsonFileManager()
        {
            _options = new JsonSerializerOptions 
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public async Task SaveToFileAsync<T>(string filePath, T data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, _options);
                await File.WriteAllTextAsync(filePath, json, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
                throw;
            }
        }


        public async Task<T?> LoadFromFileAsync<T>(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return default;

                string json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json, _options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file: {ex.Message}");
                throw;
            }
        }
    }
}