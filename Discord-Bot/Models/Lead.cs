using System.Text.Json;

namespace Discord_Bot.Models
{
    public class Lead
    {
        public ulong Id { get; set; } = 0;
        public uint GamesCount { get; set; } = 0;
        public uint GamesInCurrentMonthCount { get; set; } = 0;
        public uint Quota { get; set; } = 0;

        public Lead(ulong id)
        {
            this.Id = id;
        }

        public override string ToString()
        {
            return
                $"> `Id` → {Id}\n" +
                $"> `Количество игр` → {GamesCount}\n" +
                $"> `Количество игр в этом месяце` → {GamesInCurrentMonthCount}\n" +
                $"> `Квота`: {Quota}";
        }

        public static Lead FromJson(string fromJson)
        {
            var lead = JsonSerializer.Deserialize<Lead>(fromJson);
            if (lead == null)
                throw new JsonException("Failed to deserialize Lead");
            return lead;
        }
    }
}
