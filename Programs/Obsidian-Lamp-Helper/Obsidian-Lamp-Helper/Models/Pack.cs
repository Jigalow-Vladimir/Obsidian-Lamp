using System.Text.Json.Serialization;

namespace Obsidian_Lamp_Helper.Models
{
    public class Pack
    {
        [JsonPropertyName("�������� ������")]
        public string Name { get; set; }

        [JsonPropertyName("������ � ����������?")]
        public bool IsImagePack { get; set; }

        [JsonPropertyName("�����")]
        public List<Card> Cards { get; set; }

        public Pack(
            string name, 
            List<Card> cards,
            bool isImagePack = false) 
        {
            Name = name;
            Cards = cards;
            IsImagePack = isImagePack;
        }
    }
}