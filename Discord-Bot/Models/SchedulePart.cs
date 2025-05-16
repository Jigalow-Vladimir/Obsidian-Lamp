using System.Text.Json;

namespace Discord_Bot.Models
{
    public class SchedulePart
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public ulong LeadId { get; set; }
        public DateTime Date { get; set; }

        public SchedulePart(
            string name, 
            ulong leadId, 
            DateTime date)
        {
            this.Name = name;
            this.LeadId = leadId;
            this.Date = date;
            this.Key = $"{date.ToString($"yy.MM.dd-HH:mm-ddd")}";
        }

        public SchedulePart() 
        {
            this.Name = string.Empty;
            this.LeadId = 0;
            this.Date = DateTime.MinValue;
            this.Key = string.Empty;
        }

        public override string ToString()
        {
            return 
                $"> `Дата` → {Date}\n" +
                $"> `Игра` → {Name}\n" +
                $"> `Ведущий` → <@{LeadId}>\n" +
                $"> `Id` → {LeadId}";
        }

        public static SchedulePart SchedulePartFromJson(string fromJson)
        {
            var part = JsonSerializer.Deserialize<SchedulePart>(fromJson);
            if (part == null)
                throw new JsonException("Failed to deserialize SchedulePart");
            return part;
        }
    }
}
