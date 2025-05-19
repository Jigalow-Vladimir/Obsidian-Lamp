using Discord_Bot.Models;
using System.Text.Json;

namespace Discord_Bot.StaticModules
{
    public static class EventModule
    {
        private static readonly CloudflareApiHandler _api = new(Resources.Credentials["cloudflare-namespace-events"]);

        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public static async Task<(bool isSuccess, string result)> DeleteAsync(string key)
        {
            var (success, result) = await _api.GetAsync(key);

            if (!success || string.IsNullOrWhiteSpace(result))
                return (false, result ?? "Unexpected Error");

            var part = Part.PartFromJson(result);

            (success, result) = await _api.DeleteAsync(key);

            if (!success)
                return (false, $"delete failed: {result}");

            (success, result) = await LeadModule.DecrementEventsCountAsync(part.LeadsIds, part.StartDate);

            if (!success)
                return (false, $"failed to decrease event count: {result}");

            return (true, "delete: success");
        }

        public static async Task<(bool isSuccess, string result)> GetAsync(string key)
        {
            var (success, result) = await _api.GetAsync(key);

            if (!success || string.IsNullOrWhiteSpace(result))
                return (false, result ?? "Unexpected Error");

            return (true, result);
        }

        public static async Task<(bool isSuccess, string? result)> PutAsync(
            string name,
            ulong lead1Id,
            DateTime startDate,
            DateTime endDate,
            ulong? lead2Id,
            ulong? lead3Id,
            ulong? user1Id,
            ulong? user2Id,
            ulong? user3Id)
        {
            var leads = new List<ulong> { lead1Id };

            if (lead2Id != null)
                leads.Add((ulong)lead2Id);

            if (lead3Id != null)
                leads.Add((ulong)lead3Id);

            var users = new List<ulong>();

            if (user1Id != null)
                users.Add((ulong)user1Id);

            if (user2Id != null)
                users.Add((ulong)user2Id);

            if (user3Id != null)
                users.Add((ulong)user3Id);

            var part = new Part(name, leads, startDate, endDate, users);

            var json = JsonSerializer.Serialize(part, _jsonOptions);

            var (success, result) = await _api.PutAsync(part.Key, json);

            if (!success)
                return (false, result);

            (_, result) = await LeadModule.IncrementEventsCountAsync(leads, startDate);

            return (true, result);
        }

        public static async Task<(bool isSuccess, List<(string, string)> result)> GetListAsync(uint count)
        {
            var (successAll, rawAll) = await _api.GetAllAsync();

            if (!successAll || string.IsNullOrWhiteSpace(rawAll))
                return (false, [("failed to get list", string.Empty)]);

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll).RootElement.GetProperty("result"), _jsonOptions);

            if (keys == null)
                return (false, [("failed to get list", string.Empty)]);

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
                return (false, [("no data", "")]);

            return (true, parts.Select(p => (p.Key, p.ToString())).ToList());
        }
    }
}
