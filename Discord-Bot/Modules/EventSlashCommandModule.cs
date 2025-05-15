using Discord;
using Discord.Interactions;
using Discord_Bot.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Modules
{
    public class EventSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CloudflareApiHandler _api = new(Resources.Credentials["cloudflare-namespace-events"]);
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        [SlashCommand("event-get", "Echo an event by id")]
        public async Task Get(string key)
        {
            await RespondAsync("Process...", ephemeral: true);
            var result = await _api.GetAsync(key);

            var channel = Context.Channel as ITextChannel;

            if (channel != null)
                await channel.SendMessageAsync("Get: " + result);
        }

        [SlashCommand("event-put", "post an event")]
        public async Task Put(
            string name,
            IUser lead,
            DateTime date,
            IUser? activeUser1 = null,
            IUser? activeUser2 = null,
            IUser? activeUser3 = null)
        {
            await RespondAsync("Process...", ephemeral: true);

            var part = new Part(
                name,
                lead.Id,
                date,
                activeUser1 == null ? 0 : activeUser1.Id,
                activeUser2 == null ? 0 : activeUser2.Id,
                activeUser3 == null ? 0 : activeUser3.Id);

            string json = JsonSerializer.Serialize(part, _jsonOptions);

            await _api.PutAsync(part.Key, json);

            var channel = Context.Channel as ITextChannel;

            if (channel != null)
                await channel.SendMessageAsync($"Put: success");
        }

        [SlashCommand("event-delete", "delete an event")]
        public async Task Delete(string key)
        {
            await RespondAsync("Process...", ephemeral: true);
            await _api.DeleteAsync(key);

            var channel = Context.Channel as ITextChannel;

            if (channel != null)
                await channel.SendMessageAsync($"Delete: success");
        }

        [SlashCommand("event-list", "list events")]
        public async Task List(
            [MaxValue(EmbedBuilder.MaxFieldCount)]
            [MinValue(1)]
            uint count = 1)
        {
            await RespondAsync("Process...", ephemeral: true);
            var apiResponse = await _api.GetAllAsync();

            using var docs = JsonDocument.Parse(apiResponse);

            var channel = Context.Channel as ITextChannel;
            var apiResponseElement = docs.RootElement.GetProperty("result");

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(apiResponseElement, _jsonOptions);

            List<Part> parts = new();

            if (keys == null)
                return;

            for (int i = 0; i < count && i < keys.Count; i++)
            {
                var json = await _api.GetAsync(keys[i].Name);
                var part = JsonSerializer.Deserialize<Part>(json, _jsonOptions);

                if (part == null)
                    continue;

                parts.Add(part);
            }

            if (channel != null)
            {
                EmbedBuilder embedBuilder = new EmbedBuilder()
                    .WithTitle("События")
                    .WithColor(Color.Blue)
                    .WithFields(parts
                        .Select(p => new EmbedFieldBuilder()
                            .WithName(p.Key)
                            .WithValue(p.ToString())));

                await channel.SendMessageAsync(embed: embedBuilder.Build());
            }

        }

        public class ResultItem
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = "";
        }
    }
}