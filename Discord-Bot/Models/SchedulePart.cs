using System.Text.Json;

namespace Discord_Bot.Models
{
    public class SchedulePart
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public ulong LeadId { get; set; }
        public DateTime StartDate { get; set; }

        public SchedulePart(
            string name, 
            ulong leadId, 
            DateTime date)
        {
            this.Name = name;
            this.LeadId = leadId;
            this.StartDate = date;
            this.Key = $"{date.ToString($"yyMMddHHmmddd")}-" +
                $"{DateTime.Now.ToString("ddMMyyHHmm")}";
        }

        public SchedulePart() 
        {
            this.Name = string.Empty;
            this.LeadId = 0;
            this.StartDate = DateTime.MinValue;
            this.Key = string.Empty;
        }

        public override string ToString()
        {
            return 
                $"> `Дата начала` → {StartDate}\n" +
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
