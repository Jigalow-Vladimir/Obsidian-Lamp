using Discord;
using Discord_Bot.Models;
using System.Text.Json;
using System.Threading.Channels;

namespace Discord_Bot.StaticModules
{
    public static class EventModule
    {
        private static readonly CloudflareApiHandler _api = new(Resources.Credentials["cloudflare-namespace-events"]);
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public static async Task<(bool isSuccess, string result)> DeleteAsync(string key)
        {
            var (success, result) = await GetAsync(key);
            if (!success)
                return (false, $"Ошибка получения данных: {result}");

            var part = Part.PartFromJson(result);

            (success, result) = await _api.DeleteAsync(key);
            if (!success)
                return (false, $"Ошибка удаления: {result}");

            (success, result) = await LeadModule.DecrementEventsCountAsync(part.LeadId.ToString(), part.StartDate);

            if (!success)
                return (false, $"Ошибка уменьшения счетчика событий: {result}");

            return (true, "Delete: Success");
        }

        public static async Task<(bool isSuccess, string result)> GetAsync(string key)
        {
            var (successAll, rawAll) = await _api.GetAllAsync();
            if (!successAll || string.IsNullOrWhiteSpace(rawAll))
                return (false, "Ошибка получения списка ключей.");

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll).RootElement.GetProperty("result"), _jsonOptions);

            if (keys?.Any(k => k.Name == key) != true)
                return (false, "Ключ не найден");
            
            var (success, result) = await _api.GetAsync(key);
            if (!success || string.IsNullOrWhiteSpace(result))
                return (false, "Ошибка получения данных");

            return (true, result);
        }

        public static async Task<(bool isSuccess, string? result)> PutAsync(
            string name,
            ulong leadId,
            DateTime startDate,
            DateTime endDate,
            ulong? user1Id,
            ulong? user2Id,
            ulong? user3Id)
        {
            var part = new Part(name, leadId, startDate, endDate,
            user1Id ?? 0, user2Id ?? 0, user3Id ?? 0);

            var json = JsonSerializer.Serialize(part, _jsonOptions);
            var (success, result) = await _api.PutAsync(part.Key, json);

            if (!success)
                return (false, result);

            (success, result) = await LeadModule.IncrementEventsCountAsync(leadId.ToString(), startDate);

            return (true, result);
        }

        public static async Task<(bool isSuccess, List<(string, string)> result)> GetListAsync(uint count)
        {
            var (successAll, rawAll) = await _api.GetAllAsync();
            
            if (!successAll || string.IsNullOrWhiteSpace(rawAll))
                return (false, [("Ошибка получения списка.", string.Empty)]);

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll).RootElement.GetProperty("result"), _jsonOptions);
            
            if (keys == null)
                return (false, [("Ошибка получения списка.", string.Empty)]);

            var parts = new List<Part>();
            for (int i = 0; i < count && i < keys.Count; i++)
            {
                var (success, value) = await _api.GetAsync(keys[i].Name);
                if (!success || string.IsNullOrWhiteSpace(value))
                    continue;

                var part = JsonSerializer.Deserialize<Part>(value, _jsonOptions);
                if (part != null)
                    parts.Add(part);
            }

            if (parts.Count == 0)
                return (false, [("Нет данных.", "")]);

            return (true, parts.Select(p => (p.Key, p.ToString())).ToList());
        }
    }
}
