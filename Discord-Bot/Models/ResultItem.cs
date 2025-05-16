using System.Text.Json.Serialization;

namespace Discord_Bot.Models
{
    public class ResultItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}
