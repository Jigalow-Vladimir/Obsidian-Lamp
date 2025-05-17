using System.Text;
using System.Text.Json;

namespace Discord_Bot.Models
{
    public class Part : SchedulePart
    {
        public List<ulong> ActiveUsers { get; set; }
        public DateTime EndDate { get; set; }

        public Part(
            string name,
            ulong leadId,
            DateTime startDate,
            DateTime endTime,
            ulong activeUserId1 = 0,
            ulong activeUserId2 = 0,
            ulong activeUserId3 = 0) : 
                base(name, leadId, startDate)
        {
            this.EndDate = endTime;

            this.ActiveUsers = [];

            if (activeUserId1 != 0)
                this.ActiveUsers.Add(activeUserId1);

            if (activeUserId2 != 0)
                this.ActiveUsers.Add(activeUserId2);

            if (activeUserId3 != 0)
                this.ActiveUsers.Add(activeUserId3);
        }

        public Part()
        {
            this.ActiveUsers = new List<ulong>();
        }

        public static Part PartFromJson(string fromJson)
        {
            var part = JsonSerializer.Deserialize<Part>(fromJson);
            if (part == null)
                throw new JsonException("Failed to deserialize Part");
            return part;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());

            sb.AppendLine();
            sb.AppendLine($"> `Дата конца` → {EndDate}");

            if (this.ActiveUsers.Count == 0)
                return sb.ToString();

            sb.AppendLine("Активные игроки:");
            foreach (var user in ActiveUsers)
                if (user != 0)
                    sb.AppendLine($"> - <@{user}>");
            
            return sb.ToString();
        }

        public static Part ModifyToPart(
            SchedulePart schedulePart,
            DateTime endTime,
            ulong user1,
            ulong user2,
            ulong user3)
        {
            return new Part(
                schedulePart.Name,
                schedulePart.LeadId,
                schedulePart.StartDate,
                endTime,
                user1,
                user2,
                user3);
        }
    }
}
