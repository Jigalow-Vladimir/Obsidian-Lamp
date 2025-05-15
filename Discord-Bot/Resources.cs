using System.Text.Json;

namespace Discord_Bot
{
    public static class Resources
    {
        private const string _credentialsPath = "credentials.json";
        public static Dictionary<string, string> Credentials;

        public static async Task SetCredentialsAsync()
        {
            Credentials = await InitializeCredentials();
        }

        private static async Task<Dictionary<string, string>> InitializeCredentials()
        {
            if (!File.Exists(_credentialsPath)) return [];

            var credentials = JsonSerializer
                .Deserialize<Dictionary<string, string>>(await File
                    .ReadAllTextAsync(_credentialsPath));

            return credentials ?? [];
        }
    }
}
