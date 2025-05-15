using Discord;
using Discord.Interactions;
using Discord_Bot.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;

namespace Discord_Bot.Modules
{
    public class ScheduleSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CloudflareApiHandler _api = new(Resources.Credentials["cloudflare-namespace-schedule"]);
        private readonly CloudflareApiHandler _api2 = new(Resources.Credentials["cloudflare-namespace-events"]);
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        [SlashCommand("schedule-get", "Echo an event in schedule by id")]
        public async Task Get(string key)
        {
            await RespondAsync("Process...", ephemeral: true);
            var result = await _api.GetAsync(key);

            var channel = Context.Channel as ITextChannel;

            if (channel != null)
                await channel.SendMessageAsync("Get: " + result);
        }

        [SlashCommand("schedule-put", "put an event in schedule")]
        public async Task Put(
            string name,
            IUser lead,
            DateTime date)
        {
            await RespondAsync("Process...", ephemeral: true);

            var schedulePart = new SchedulePart(
                name,
                lead.Id,
                date);

            string json = JsonSerializer.Serialize(schedulePart, _jsonOptions);

            await _api.PutAsync(schedulePart.Key, json);

            var channel = Context.Channel as ITextChannel;

            if (channel != null)
                await channel.SendMessageAsync($"Put: success");
        }

        [SlashCommand("schedule-delete", "delete an event in schedule")]
        public async Task Delete(string key)
        {
            await RespondAsync("Process...", ephemeral: true);
            await _api.DeleteAsync(key);

            var channel = Context.Channel as ITextChannel;

            if (channel != null)
                await channel.SendMessageAsync($"Delete: success");
        }

        [SlashCommand("schedule-list", "list all events in schedule")]
        public async Task List(
            [MaxValue(EmbedBuilder.MaxFieldCount)]
            [MinValue(1)]
            uint count = 1)
        {
            await RespondAsync("Process...", ephemeral: true);
            var result = await _api.GetAllAsync();

            using var docs = JsonDocument.Parse(result);

            var channel = Context.Channel as ITextChannel;
            var resultElement = docs.RootElement.GetProperty("result");

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(resultElement, _jsonOptions);

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
                    .WithTitle("Расписание")
                    .WithColor(Color.Blue)
                    .WithFields(parts
                        .Select(p => new EmbedFieldBuilder()
                            .WithName(p.Key)
                            .WithValue(p.ToString())));

                await channel.SendMessageAsync(embed: embedBuilder.Build());
            }
        }

        [SlashCommand("schedule-complete", "complete an event in schedule")]
        public async Task Complete(
            string key, 
            IUser? activeUser1 = null, 
            IUser? activeUser2 = null, 
            IUser? activeUser3 = null)
        {
            await RespondAsync("Process...", ephemeral: true);
            var result = await _api.GetAsync(key);

            var schedulePart = JsonSerializer.Deserialize<SchedulePart>(result, _jsonOptions);
            if (schedulePart == null)
                return;

            var part = new Part(
                schedulePart.Name,
                schedulePart.LeadId,
                schedulePart.Date, 
                activeUser1 == null ? 0 : activeUser1.Id,
                activeUser2 == null ? 0 : activeUser2.Id,
                activeUser3 == null ? 0 : activeUser3.Id);

            if (part == null)
                return;

            string json = JsonSerializer.Serialize(part, _jsonOptions);

            await _api2.PutAsync(schedulePart.Key, json);
            await _api.DeleteAsync(key);

            var channel = Context.Channel as ITextChannel;
            if (channel != null)
                await channel.SendMessageAsync("Complete: " + result);
        }

        public class ResultItem
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = "";
        }
    }
}
