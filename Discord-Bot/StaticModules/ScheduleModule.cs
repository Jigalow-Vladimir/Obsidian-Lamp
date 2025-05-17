using Discord_Bot.Models;
using System.Text.Json;
using System.Threading.Channels;

namespace Discord_Bot.StaticModules
{
    public static class ScheduleModule
    {
        private static readonly CloudflareApiHandler _apiSchedule = new(Resources.Credentials["cloudflare-namespace-schedule"]);
        private static readonly CloudflareApiHandler _apiEvents = new(Resources.Credentials["cloudflare-namespace-events"]);
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public static async Task<(bool Success, string Result)> GetAsync(string key) 
        {
            var (successAll, rawAll) = await _apiSchedule.GetAllAsync();

            if (!successAll || string.IsNullOrWhiteSpace(rawAll))
                return (false, "Ошибка получения списка ключей.");
            
            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll).RootElement.GetProperty("result"), _jsonOptions);
            
            if (keys?.Any(k => k.Name == key) != true)
                return (false, "Ключ не найден");
            
            var (success, result) = await _apiSchedule.GetAsync(key);
            
            if (!success || string.IsNullOrWhiteSpace(result))
                return (false, "Ошибка получения данных");
            
            return (true, result);
        }
        
        public static async Task<(bool Success, string Result)> PutAsync(string name, ulong leadId, DateTime date) 
        {
            var part = new SchedulePart(name, leadId, date);

            var json = JsonSerializer.Serialize(part, _jsonOptions);
            
            var (success, result) = await _apiSchedule.PutAsync(part.Key, json);
            
            if (!success)
                return (false, result ?? "Error");
            
            return (true, "Put: Success");
        }

        public static async Task<(bool Success, string Result)> DeleteAsync(string key) 
        {
            var (success, result) = await _apiSchedule.DeleteAsync(key);
            if (!success)
                return (false, $"Ошибка удаления: {result}");

            return (true, "Delete: Success");
        }

        public static async Task<(bool Success, List<(string Result, string Value)> Result)> ListAsync(uint count)
        {
            var (success, result) = await _apiSchedule.GetAllAsync();
            
            if (!success || string.IsNullOrWhiteSpace(result))
                return (false, [("Ошибка получения списка ключей.", string.Empty)]);
            
            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(result).RootElement.GetProperty("result"), _jsonOptions);

            if (keys == null || keys.Count == 0)
                return (false, [("Ошибка получения списка.", string.Empty)]);

            var parts = new List<Part>();
            for (int i = 0; i < count && i < keys.Count; i++)
            {
                (success, result) = await _apiSchedule.GetAsync(keys[i].Name);
                if (!success || string.IsNullOrWhiteSpace(result))
                    continue;

                var part = JsonSerializer.Deserialize<Part>(result, _jsonOptions);
                if (part != null)
                    parts.Add(part);
            }

            if (parts.Count == 0)
                return (false, [("Нет данных.", "")]);

            return (true, parts.Select(p => (p.Key, p.ToString())).ToList());
        }

        public static async Task<(bool Success, string Result)> CompleteAsync(string key,
            DateTime endTime, ulong? user1Id, ulong? user2Id, ulong? user3Id)
        {
            var (success, rawPart) = await _apiSchedule.GetAsync(key);
            if (!success)
                return (false, $"Ошибка получения события: {rawPart}");

            var schedulePart = JsonSerializer.Deserialize<SchedulePart>(rawPart!, _jsonOptions);
            if (schedulePart == null)
                return (false, "Ошибка при десериализации события");

            var part = Part.ModifyToPart(
                schedulePart,
                endTime,
                user1Id ?? 0,
                user2Id ?? 0,
                user3Id ?? 0);

            if (part == null)
                return (false, "Ошибка при формировании завершённого события");

            var json = JsonSerializer.Serialize(part, _jsonOptions);

            var (putSuccess, putError) = await _apiEvents.PutAsync(schedulePart.Key, json);
            if (!putSuccess)
                return (false, $"Ошибка сохранения события: {putError}");

            var (deleteSuccess, deleteError) = await _apiSchedule.DeleteAsync(key);
            if (!deleteSuccess)
                return (false, $"Ошибка удаления исходного события: {deleteError}");

            return (true, "Complete: Success");
        }
    }
}
