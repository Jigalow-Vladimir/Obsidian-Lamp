using System.Text.Json.Serialization;

namespace Obsidian_Lamp_Helper.Models
{
    public class Card
    {
        [JsonPropertyName("Значение")]
        public string Content { get; set; }

        public Card(
            string content)
        {
            Content = content;
        }
    }
}