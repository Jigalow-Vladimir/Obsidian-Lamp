using Discord_Bot.Models;
using System.Text.Json;

namespace Discord_Bot.StaticModules
{
    public static class LeadModule
    {
        private static readonly CloudflareApiHandler _api = new(Resources.Credentials["cloudflare-namespace-leads"]);

        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        private static async Task<(bool Success, List<ResultItem>? Keys, string? Error)> GetKeysAsync()
        {
            var (successAll, rawAll) = await _api.GetAllAsync();

            if (!successAll || string.IsNullOrWhiteSpace(rawAll))
                return (false, null, "error getting key list");

            try
            {
                var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                    JsonDocument.Parse(rawAll).RootElement.GetProperty("result"), _jsonOptions);
        
                return (true, keys, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"error parsing keys: {ex.Message}");
            }
        }

        private static async Task<(bool Success, string? Error)> UpdateLeadAsync(string key, Func<Lead, bool> updateAction)
        {
            var (success, json) = await _api.GetAsync(key);
        
            if (!success || string.IsNullOrWhiteSpace(json))
                return (false, "error getting data");

            var lead = Lead.FromJson(json);
        
            var changed = updateAction(lead);
        
            if (!changed)
                return (true, null);

            var newJson = JsonSerializer.Serialize(lead, _jsonOptions);
        
            var (putSuccess, error) = await _api.PutAsync(key, newJson);
        
            return putSuccess ? (true, null) : (false, error);
        }


        public static async Task<(bool, string)> GetAsync(string key)
        {
            var (getSuccess, result) = await _api.GetAsync(key);
        
            if (!getSuccess || string.IsNullOrWhiteSpace(result))
                return (false, "error getting data");

            return (true, result);
        }

        public static async Task<(bool, string?)> IncrementEventsCountAsync(List<ulong> leadsIds, DateTime startTime)
        {
            foreach (var key in leadsIds)
            {
                var (success, error) = await UpdateLeadAsync(key.ToString(), lead =>
                {
                    lead.GamesCount++;
        
                    if (startTime.Month == DateTime.Now.Month)
                        lead.GamesInCurrentMonthCount++;
        
                    return true;
                });

                if (!success)
                    return (false, error);
            }

            return (true, "put: success");
        }

        public static async Task<(bool, string?)> DecrementEventsCountAsync(List<ulong> leadsIds, DateTime startTime)
        {
            foreach (var key in leadsIds)
            {
                var (success, error) = await UpdateLeadAsync(key.ToString(), lead =>
                {
                    lead.GamesCount = Math.Max(0, lead.GamesCount - 1);
        
                    if (startTime.Month == DateTime.Now.Month)
                        lead.GamesInCurrentMonthCount = Math.Max(0, lead.GamesInCurrentMonthCount - 1);
        
                    return true;
                });

                if (!success)
                    return (false, error);
            }

            return (true, "put: success");
        }

        public static async Task<(bool, string)> PutAsync(ulong id)
        {
            var (success, error) =
                await _api
                    .PutAsync(id
                    .ToString(), JsonSerializer
                    .Serialize(new Lead(id), _jsonOptions));

            if (!success)
                return (false, $"error adding: {error}");

            return (true, "put: success");
        }

        public static async Task<(bool Success, List<(string ResultOrName, string Value)>)> GetListAsync()
        {
            var (successAll, rawAll) = await _api.GetAllAsync();

            if (!successAll)
                return (false, [($"error getting key list: {rawAll}", string.Empty)]);

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll!).RootElement.GetProperty("result"), _jsonOptions);

            if (keys == null || keys.Count == 0)
                return (false, [("no keys found", string.Empty)]);

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
            var (success, keys, error) = await GetKeysAsync();
        
            if (!success || keys?.All(k => k.Name != key) == true)
                return (false, error ?? "key not found");

            var (deleteSuccess, err) = await _api.DeleteAsync(key);
        
            return deleteSuccess ? (true, "delete: success") : (false, err ?? "error");
        }

        public static async Task<(bool Success, string Result)> NewMonthAsync()
        {
            var (success, keys, error) = await GetKeysAsync();

            if (!success || keys == null || keys.Count == 0)
                return (false, error ?? "no keys found");

            foreach (var key in keys)
            {
                var (updateSuccess, updateError) = await UpdateLeadAsync(key.Name, lead =>
                {
                    lead.GamesInCurrentMonthCount = 0;
                    return true;
                });

                if (!updateSuccess)
                    return (false, updateError ?? "error updating");
            }

            return (true, "new month: success");
        }

        public static async Task<(bool Success, string Result)> QuotaAsync(string key, uint newQuota)
        {
            var (success, keys, error) = await GetKeysAsync();

            if (!success || keys?.All(k => k.Name != key) == true)
                return (false, error ?? "key not found");

            var (updateSuccess, updateError) = await UpdateLeadAsync(key, lead =>
            {
                lead.Quota = newQuota;
                return true;
            });

            return updateSuccess ? (true, "put: success") : (false, updateError ?? "error updating");
        }
    }
}
