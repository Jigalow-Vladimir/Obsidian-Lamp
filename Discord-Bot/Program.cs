using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot
{
    public static class Program
    {
        private static DiscordSocketClient _client = new ();
        private static IServiceProvider? _service = null;
        private static InteractionService? _interactionService = null;

        public static async Task Main()
        {
            _client.Log += Log;
            _client.Ready += OnReady;

            _interactionService = new InteractionService(_client);

            _service = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_interactionService)
                .BuildServiceProvider();

            _client.InteractionCreated += OnInteractionCreated;

            await Resources.SetCredentialsAsync();

            await _client.LoginAsync(TokenType.Bot, Resources.Credentials["discord-bot-token"]);
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
            if (ulong.TryParse(Resources.Credentials["discord-guild-id"], out ulong guildId) && _interactionService != null)
            {
                await _interactionService.AddModulesAsync(typeof(Program).Assembly, _service);
                await _interactionService.RegisterCommandsToGuildAsync(guildId);
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
    }
}
