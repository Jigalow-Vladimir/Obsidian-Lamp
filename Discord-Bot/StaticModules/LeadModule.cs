using Discord;
using Discord_Bot.Models;
using System.Text.Json;
using System.Threading.Channels;

namespace Discord_Bot.StaticModules
{
    public static class LeadModule
    {
        private static readonly CloudflareApiHandler _api = new(Resources.Credentials["cloudflare-namespace-leads"]);
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public static async Task<(bool, string)> GetAsync(string key)
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

        /// Increment the events count for a lead <param name="key">Lead ID</param>
        public static async Task<(bool, string?)> IncrementEventsCountAsync(string key, DateTime startTime)
        {
            var (success, result) = await GetAsync(key);
            if (!success)
                return (false, result);

            var lead = Lead.FromJson(result);
            lead.GamesCount++;

            if (startTime.Month == DateTime.Now.Month)
                lead.GamesInCurrentMonthCount++;

            var json = JsonSerializer.Serialize(lead, _jsonOptions);
            
            var (putSuccess, error) = await _api.PutAsync(key, json);
            if (!putSuccess)
                return (false, error);

            return (true, "Put: Success");
        }

        /// Decrement the events count for a lead <param name="key">Lead ID</param>
        public static async Task<(bool, string?)> DecrementEventsCountAsync(string key, DateTime startTime)
        {
            var (success, result) = await GetAsync(key);
            if (!success)
                return (false, result);

            var lead = Lead.FromJson(result);
            lead.GamesCount--;

            if (startTime.Month == DateTime.Now.Month)
                lead.GamesInCurrentMonthCount--;

            var json = JsonSerializer.Serialize(lead, _jsonOptions);

            var (putSuccess, error) = await _api.PutAsync(key, json);
            if (!putSuccess)
                return (false, error);

            return (true, "Put: Success");
        }

        public static async Task<(bool, string)> PutAsync(ulong id)
        {
            var (success, error) = 
                await _api
                    .PutAsync(id
                    .ToString(), JsonSerializer
                    .Serialize(new Lead(id), _jsonOptions));

            if (!success)
                return (false, $"Ошибка при добавлении: {error}");
        
            return (true, "Put: Success");
        }

        public static async Task<(bool Success, List<(string ResultOrName, string Value)>)> GetListAsync()
        {
            var (successAll, rawAll) = await _api.GetAllAsync();

            if (!successAll)
                return (false, [($"Ошибка получения списка ключей: {rawAll}", string.Empty)]);

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll!).RootElement.GetProperty("result"), _jsonOptions);

            if (keys == null || keys.Count == 0)
                return (false, [("Ключи не найдены", string.Empty)]);

            var leads = new List<Lead>();
            foreach (var key in keys)
            {
                var json = await _api.GetAsync(key.Name);

                if (!json.IsSuccess || json.Result == null)
                    continue;

                leads.Add(Lead.FromJson(json.Result));
            }

            var result = new List<(string, string)>();
            
            foreach (var lead in leads)
                result.Add((lead.Id.ToString(), lead.ToString()));
            
            return (true, result);
        }

        public static async Task<(bool Success, string Result)> DeleteAsync(string key)
        {
            var (successAll, rawAll) = await _api.GetAllAsync();
            if (!successAll)
                return (false, "Ошибка получения списка ключей.");

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll!).RootElement.GetProperty("result"), _jsonOptions);
            
            if (keys == null || keys.Count == 0)
                return (false, "Ключи не найдены");
            
            if (keys.All(k => k.Name != key))
                return (false, "Ключ не найден");
            
            var (success, error) = await _api.DeleteAsync(key);

            if (!success)
                return (false, error ?? "Error");

            return (true, "Delete: Success");
        }

        public static async Task<(bool Success, string Result)> NewMonthAsync()
        {
            var (successAll, rawAll) = await _api.GetAllAsync();
            
            if (!successAll)
                return (false, "Ошибка получения списка ключей.");

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll!).RootElement.GetProperty("result"), _jsonOptions);

            if (keys == null || keys.Count == 0)
                return (false, "Ключи не найдены");

            foreach (var key in keys)
            {
                var json = await _api.GetAsync(key.Name);
                if (!json.IsSuccess || json.Result == null)
                    continue;
                var lead = Lead.FromJson(json.Result);
                lead.GamesInCurrentMonthCount = 0;
                var newJson = JsonSerializer.Serialize(lead, _jsonOptions);
                await _api.PutAsync(key.Name, newJson);
            }
            return (true, "New month: Success");
        }

        public static async Task<(bool Success, string Result)> Quota(string key, uint newQuota)
        {
            var (successAll, rawAll) = await _api.GetAllAsync();

            if (!successAll)
                return (false, "Ошибка получения списка ключей.");

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll!).RootElement.GetProperty("result"), _jsonOptions);

            if (keys == null || keys.Count == 0)
                return (false, "Ключи не найдены");

            if (keys.All(k => k.Name != key))
                return (false, "Ключ не найден");

            var json = await _api.GetAsync(key);
            if (!json.IsSuccess || json.Result == null)
                return (false, "Ошибка получения данных");

            var lead = Lead.FromJson(json.Result);
            lead.Quota = newQuota;

            var newJson = JsonSerializer.Serialize(lead, _jsonOptions);
            var (putSuccess, error) = await _api.PutAsync(key, newJson);
            
            if (!putSuccess)
                return (false, error ?? "Error");

            return (true, "Put: Success");
        }
    }
}
