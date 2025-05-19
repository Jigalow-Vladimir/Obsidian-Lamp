using Discord_Bot.Models;
using System.Text.Json;

namespace Discord_Bot.StaticModules
{
    public static class ScheduleModule
    {
        private static readonly CloudflareApiHandler _apiSchedule = new(Resources.Credentials["cloudflare-namespace-schedule"]);

        private static readonly CloudflareApiHandler _apiEvents = new(Resources.Credentials["cloudflare-namespace-events"]);

        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public static async Task<(bool Success, string Result)> GetAsync(string key)
        {
            var (success, result) = await _apiSchedule.GetAsync(key);

            if (!success || string.IsNullOrWhiteSpace(result))
                return (false, "failed to get data");

            return (true, result);
        }

        public static async Task<(bool Success, string Result)> PutAsync(
            string name,
            ulong lead1Id,
            DateTime date,
            ulong? lead2Id,
            ulong? lead3Id)
        {
            var leads = new List<ulong> { lead1Id };

            if (lead2Id != null)
                leads.Add((ulong)lead2Id);

            if (lead3Id != null)
                leads.Add((ulong)lead3Id);

            var part = new SchedulePart(name, leads, date);

            var json = JsonSerializer.Serialize(part, _jsonOptions);

            var (success, result) = await _apiSchedule.PutAsync(part.Key, json);

            if (!success)
                return (false, result ?? "error");

            return (true, "put: success");
        }

        public static async Task<(bool Success, string Result)> DeleteAsync(string key)
        {
            var (success, result) = await _apiSchedule.DeleteAsync(key);

            if (!success)
                return (false, $"delete failed: {result}");

            return (true, "delete: success");
        }

        public static async Task<(bool Success, List<(string Result, string Value)> Result)> ListAsync(uint count, bool isPretty = false)
        {
            var (success, result) = await _apiSchedule.GetAllAsync();

            if (!success || string.IsNullOrWhiteSpace(result))
                return (false, [("failed to get key list", string.Empty)]);

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(result).RootElement.GetProperty("result"), _jsonOptions);

            if (keys == null || keys.Count == 0)
                return (false, [("no keys found", string.Empty)]);

            var parts = new List<SchedulePart>();

            for (int i = 0; i < count && i < keys.Count; i++)
            {
                (success, result) = await _apiSchedule.GetAsync(keys[i].Name);

                if (!success || string.IsNullOrWhiteSpace(result))
                    continue;

                var part = JsonSerializer.Deserialize<SchedulePart>(result, _jsonOptions);

                if (part != null)
                    parts.Add(part);
            }

            if (parts.Count == 0)
                return (false, [("no data", "")]);

            (bool, List<(string, string)>) resultList;

            if (isPretty)
                resultList = (true, parts.Select(p => (p.Key, p.ToStringPretty())).ToList());

            else resultList = (true, parts.Select(p => (p.Key, p.ToString())).ToList());

            return resultList;
        }

        public static async Task<(bool Success, string Result)> CompleteAsync(string key,
            DateTime endTime, ulong? user1Id, ulong? user2Id, ulong? user3Id)
        {
            var (success, rawPart) = await _apiSchedule.GetAsync(key);

            if (!success)
                return (false, $"failed to get event: {rawPart}");

            var schedulePart = JsonSerializer.Deserialize<SchedulePart>(rawPart!, _jsonOptions);

            if (schedulePart == null)
                return (false, "deserialization failed");

            var usersIds = new List<ulong>();

            if (user1Id != null)
                usersIds.Add((ulong)user1Id);

            if (user2Id != null)
                usersIds.Add((ulong)user2Id);

            if (user3Id != null)
                usersIds.Add((ulong)user3Id);

            var part = Part.ModifyToPart(schedulePart, endTime, usersIds);

            if (part == null)
                return (false, "failed to build completed event");

            var json = JsonSerializer.Serialize(part, _jsonOptions);

            var (putSuccess, putError) = await _apiEvents.PutAsync(schedulePart.Key, json);

            if (!putSuccess)
                return (false, $"save failed: {putError}");

            var (deleteSuccess, deleteError) = await _apiSchedule.DeleteAsync(key);

            if (!deleteSuccess)
                return (false, $"original delete failed: {deleteError}");

            return (true, "complete: success");
        }
    }
}
