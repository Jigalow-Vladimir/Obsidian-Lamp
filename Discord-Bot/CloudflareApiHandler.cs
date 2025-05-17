namespace Discord_Bot
{
    public sealed class CloudflareApiHandler
    {
        private readonly HttpClient httpClient;
        private const string _baseAdress = "https://api.cloudflare.com/client/v4";
        private readonly string _namespaceName;

        public CloudflareApiHandler(string namespaceName)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                    Resources.Credentials["cloudflare-api-key"]);

            _namespaceName = namespaceName;

            string uri =
                $"{_baseAdress.TrimEnd('/')}/accounts/" +
                $"{Resources.Credentials["cloudflare-account-id"]}" +
                $"/storage/kv/namespaces/" +
                $"{namespaceName}" +
                $"/values/";

            httpClient.BaseAddress = new Uri(uri);
        }

        public async Task<(bool IsSuccess, string? Result)> GetAsync(string key)
        {
            try
            {
                var response = await httpClient.GetAsync(key);
                if (!response.IsSuccessStatusCode)
                    return (false, $"GET error: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                return (true, content ?? "");
            }
            catch (Exception ex)
            {
                return (false, $"GET exception: {ex.Message}");
            }
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> PutAsync(string key, string value)
        {
            try
            {
                if (httpClient.BaseAddress == null)
                    return (false, "BaseAddress is null");

                var content = new StringContent(value);
                var response = await httpClient.PutAsync(new Uri(httpClient.BaseAddress, key), content);

                if (!response.IsSuccessStatusCode)
                    return (false, $"PUT error: {response.StatusCode}");

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"PUT exception: {ex.Message}");
            }
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteAsync(string key)
        {
            try
            {
                if (httpClient.BaseAddress == null)
                    return (false, "BaseAddress is null");

                var response = await httpClient.DeleteAsync(new Uri(httpClient.BaseAddress, key));

                if (!response.IsSuccessStatusCode)
                    return (false, $"DELETE error: {response.StatusCode}");

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"DELETE exception: {ex.Message}");
            }
        }

        public async Task<(bool IsSuccess, string? Result)> GetAllAsync()
        {
            try
            {
                string uri =
                    $"{_baseAdress.TrimEnd('/')}/accounts/" +
                    $"{Resources.Credentials["cloudflare-account-id"]}" +
                    $"/storage/kv/namespaces/" +
                    $"{_namespaceName}" +
                    $"/keys";

                var response = await httpClient.GetAsync(uri);
                if (!response.IsSuccessStatusCode)
                    return (false, $"GET ALL error: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                return (true, content ?? "");
            }
            catch (Exception ex)
            {
                return (false, $"GET ALL exception: {ex.Message}");
            }
        }
    }
}