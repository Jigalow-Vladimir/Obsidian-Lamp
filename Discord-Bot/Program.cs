using System.Text.Json;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot
{
    public class Program
    {
        private const string _credentialsPath = "credentials.json";
        private static Dictionary<string, string> _credentials = [];
        private static DiscordSocketClient _client = new ();
        private static IServiceProvider? _service = null;
        private static InteractionService? _interactionService = null;

        public static async Task Main()
        {
            var credentialsInit = InitializeCredentials();
         
            _client.Log += Log;
            _client.Ready += OnReady;

            _interactionService = new InteractionService(_client);

            _service = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_interactionService)
                .BuildServiceProvider();

            _credentials = await credentialsInit;

            _client.InteractionCreated += OnInteractionCreated;

            await _client.LoginAsync(TokenType.Bot, _credentials["discord-bot-token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.ToString());
            return Task.CompletedTask;
        }

        private static async Task OnReady()
        {
            if (ulong.TryParse(_credentials["discord-guild-id"], out ulong guildId))
            {
                if (_interactionService != null)
                {
                    await _interactionService.AddModulesAsync(typeof(Program).Assembly, _service);
                    await _interactionService.RegisterCommandsToGuildAsync(guildId);
                }
            }

            Console.WriteLine("Ready");
        }

        private static async Task OnInteractionCreated(SocketInteraction interaction)
        {
            if (_interactionService != null)
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(context, _service);
            }
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
