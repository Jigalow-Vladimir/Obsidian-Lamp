using System.Text.Json.Serialization;

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
            this.Key = $"{date.DayOfWeek}-" +
                $"{date.Date.Day}.{date.Date.Month}.{date.Date.Year}-" +
                $"{Guid.NewGuid().ToString("N")}";
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
    }
}
