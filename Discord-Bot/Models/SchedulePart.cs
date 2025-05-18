using System.Text;
using System.Text.Json;

namespace Discord_Bot.Models
{
    public class SchedulePart
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public List<ulong> LeadsIds { get; set; }
        public DateTime StartDate { get; set; }

        public SchedulePart(
            string name, 
            List<ulong> leadsIds, 
            DateTime date)
        {
            this.Name = name;
            this.LeadsIds = [];

            if (leadsIds != null)
            {
                foreach (var leadId in leadsIds)
                    this.LeadsIds.Add((leadId));
            }

            this.StartDate = date;
            this.Key = $"{date.ToString($"yyMMddHHmmddd")}-" +
                $"{DateTime.Now:ddMMyyHHmm}";
        }

        public SchedulePart() 
        {
            this.Name = string.Empty;
            this.LeadsIds = [];
            this.StartDate = DateTime.MinValue;
            this.Key = string.Empty;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"> `Дата начала` → {StartDate}");
            sb.AppendLine($"> `Игра` → {Name}");

            if (this.LeadsIds.Count == 0)
                return sb.ToString();

            sb.AppendLine("> Ведущие:");

            foreach (var leadId in LeadsIds)
                sb.AppendLine($"> - <@{leadId}>");

            return sb.ToString();
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
