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

        public async Task<string> GetAsync(string key)
        {
            var result = await (await httpClient.GetAsync(key)).Content.ReadAsStringAsync();
            return result ?? "";
        }

        public async Task PutAsync(string key, string value)
        {
            var content = new StringContent(value);

            if (httpClient.BaseAddress == null)
                return;

            await httpClient.PutAsync(new Uri(httpClient.BaseAddress, key), content);
        }

        public async Task DeleteAsync(string key)
        {
            if (httpClient.BaseAddress == null)
                return;

            await httpClient.DeleteAsync(new Uri(httpClient.BaseAddress, key));
        }

        public async Task<string> GetAllAsync()
        {
            string uri =
                $"{_baseAdress.TrimEnd('/')}/accounts/" +
                $"{Resources.Credentials["cloudflare-account-id"]}" +
                $"/storage/kv/namespaces/" +
                $"{_namespaceName}" +
                $"/keys";

            var result = await (await httpClient.GetAsync(uri)).Content.ReadAsStringAsync();
            return result;
        }
    }
}