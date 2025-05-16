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
        private readonly CloudflareApiHandler _apiSchedule = new(Resources.Credentials["cloudflare-namespace-schedule"]);
        private readonly CloudflareApiHandler _apiEvents = new(Resources.Credentials["cloudflare-namespace-events"]);
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        [SlashCommand("schedule-get", "Выводит событие в расписании по ключу")]
        public async Task Get(string key)
        {
            await RespondAsync("Process...", ephemeral: true);
            var result = await _apiSchedule.GetAsync(key);

            var channel = Context.Channel as ITextChannel;

            if (channel != null)
                await channel.SendMessageAsync("Get: " + result);
        }

        [SlashCommand("schedule-put", "Добавляет событие в расписания")]
        public async Task Put(
            [Summary("Событие")] string name,
            [Summary("Ведущий")] IUser lead,
            [Summary("Дата", "Устанавливать по этому шаблону: `dd.mm.yyyy hh:mm`")] DateTime date)
        {
            await RespondAsync("Process...", ephemeral: true);

            var part = new SchedulePart(
                name,
                lead.Id,
                date);

            await _apiSchedule.PutAsync(part.Key, 
                JsonSerializer.Serialize(part, _jsonOptions));

            var channel = Context.Channel as ITextChannel;
            if (channel != null)
                await channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("Put: Success")
                    .WithColor(Color.Green)
                    .WithDescription(part.ToString()).Build());
        }

        [SlashCommand("schedule-delete", "Удаляет событие из расписания")]
        public async Task Delete(
            [Summary("Ключ")] string key)
        {
            await RespondAsync("Process...", ephemeral: true);

            await _apiSchedule.DeleteAsync(key);

            var channel = Context.Channel as ITextChannel;
            if (channel != null)
                await channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("Delete: Success")
                    .WithColor(Color.Red).Build());
        }

        [SlashCommand("schedule-list", "Выводит события в расписании")]
        public async Task List(
            [Summary("Количество", "Максимум 25")]
            [MaxValue(EmbedBuilder.MaxFieldCount)]
            [MinValue(1)]
            uint count = 1)
        {
            await RespondAsync("Process...", ephemeral: true);

            var keys = JsonSerializer
                .Deserialize<List<ResultItem>>(JsonDocument
                    .Parse(await _apiSchedule.GetAllAsync()).RootElement
                        .GetProperty("result"), _jsonOptions);

            if (keys == null)
                return;

            List<Part> parts = new();
            for (int i = 0; i < count && i < keys.Count; i++)
            {
                var part = JsonSerializer.Deserialize<Part>(
                    await _apiSchedule.GetAsync(keys[i].Name), _jsonOptions);

                if (part == null)
                    continue;

                parts.Add(part);
            }

            var channel = Context.Channel as ITextChannel;
            if (channel != null)
                await channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("События")
                    .WithColor(Color.Blue)
                    .WithFields(parts
                        .Select(p => new EmbedFieldBuilder()
                            .WithName(p.Key)
                            .WithValue(p.ToString()))).Build());
        }

        [SlashCommand("schedule-complete", "Подтверждает выполнение события в расписании")]
        public async Task Complete(
            [Summary("ключ")] string key, 
            [Summary("активный_участник_1")] IUser? activeUser1 = null,
            [Summary("активный_участник_2")] IUser? activeUser2 = null,
            [Summary("активный_участник_3")] IUser? activeUser3 = null)
        {
            await RespondAsync("Process...", ephemeral: true);
            
            var schedulePart = JsonSerializer.Deserialize<SchedulePart>(
                await _apiSchedule.GetAsync(key), _jsonOptions);

            if (schedulePart == null)
                return;

            var part = Part.GetModified(
                schedulePart,
                activeUser1 == null ? 0 : activeUser1.Id,
                activeUser2 == null ? 0 : activeUser2.Id,
                activeUser3 == null ? 0 : activeUser3.Id);

            if (part == null)
                return;

            await _apiEvents.PutAsync(schedulePart.Key, 
                JsonSerializer.Serialize(part, _jsonOptions));

            await _apiSchedule.DeleteAsync(key);

            var channel = Context.Channel as ITextChannel;
            if (channel != null)
                await channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("Complete: Success")
                    .WithColor(Color.Green)
                    .Build());
        }
    }
}
