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
            List<ulong> leadsIds,
            DateTime startDate,
            DateTime endTime,
            List<ulong> activeUsersIds) : 
                base(name, leadsIds, startDate)
        {
            this.EndDate = endTime;

            this.ActiveUsers = [];

            if (activeUsersIds != null)
            {
                foreach (var userId in activeUsersIds)
                    this.ActiveUsers.Add(userId);
            }
        }

        public Part()
        {
            this.ActiveUsers = [];
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
            var sb = new StringBuilder();

            sb.AppendLine($"> `date of start` → {StartDate.ToString(Consts.DateFormat)}");
            sb.AppendLine($"> `date of end` → {EndDate.ToString(Consts.DateFormat)}");
            sb.AppendLine($"> `event` → {Name}");

            if (this.LeadsIds.Count == 0)
                return sb.ToString();

            sb.AppendLine("> `leads`: ");

            foreach (var leadId in LeadsIds)
                sb.AppendLine($"> → <@{leadId}>");

            if (this.ActiveUsers.Count > 0)
            {
                sb.AppendLine("> `active users`:");
                foreach (var user in ActiveUsers)
                    sb.AppendLine($"> - <@{user}>");
            }

            return sb.ToString();
        }

        public static Part ModifyToPart(
            SchedulePart schedulePart,
            DateTime endTime,
            List<ulong> usersIds)
        {
            return new Part(
                schedulePart.Name,
                schedulePart.LeadsIds,
                schedulePart.StartDate,
                endTime,
                usersIds);
        }
    }
}
