using Discord;
using Discord.Interactions;
using Discord_Bot.Models;
using Discord_Bot.StaticModules;
using System.Text.Json;

namespace Discord_Bot.SlashCommandModules
{
    [Group("schedule", "Команды для работы с расписанием")]
    public class ScheduleSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CloudflareApiHandler _apiSchedule = new(Resources.Credentials["cloudflare-namespace-schedule"]);
        private readonly CloudflareApiHandler _apiEvents = new(Resources.Credentials["cloudflare-namespace-events"]);
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        [SlashCommand("get", "Выводит событие в расписании по ключу")]
        public async Task Get([Summary("ключ")] string key)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            var (successAll, rawAll) = await _apiSchedule.GetAllAsync();
            if (!successAll)
            {
                await RespondAsync($"Ошибка получения списка ключей: {rawAll}", ephemeral: true);
                return;
            }

            var keys = JsonSerializer.Deserialize<List<ResultItem>>(
                JsonDocument.Parse(rawAll!).RootElement.GetProperty("result"), _jsonOptions);

            if (keys == null || !keys.Any(k => k.Name == key))
            {
                await RespondAsync("Ключ не найден", ephemeral: true);
                return;
            }

            var (success, result) = await _apiSchedule.GetAsync(key);
            if (!success)
            {
                await RespondAsync($"Ошибка получения данных: {result}", ephemeral: true);
                return;
            }

            await channel.SendMessageAsync("Get: " + result);
        }

        [SlashCommand("put", "Добавляет событие в расписание")]
        public async Task Put(
            [Summary("событие")] string name, 
            [Summary("ведущий")] IUser lead, 
            [Summary("дата_начала")] DateTime date)
        {
            await RespondAsync("Process...", ephemeral: true);
            
            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            var (success, result) = await ScheduleModule.PutAsync(name, lead.Id, date);
            if (!success)
            {
                await channel.SendMessageAsync($"Ошибка при добавлении: {result}");
                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle(result)
                .WithColor(Color.Green)
                .Build());
        }

        [SlashCommand("delete", "Удаляет событие из расписания")]
        public async Task Delete([Summary("ключ")] string key)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            var (success, result) = await ScheduleModule.DeleteAsync(key);
            if (!success)
            {
                await RespondAsync(result);
                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle(result)
                .WithColor(Color.Red).Build());
        }

        [SlashCommand("list", "Выводит события в расписании")]
        public async Task List(
            [Summary("количество", "Максимум 25")]
            [MaxValue(EmbedBuilder.MaxFieldCount)]
            [MinValue(1)]
            uint count = 1)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            var (successAll, rawAll) = await ScheduleModule.ListAsync(count);

            if (!successAll)
            {
                await channel.SendMessageAsync($"Ошибка получения списка событий: {rawAll[0].Result}");
                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("События")
                .WithColor(Color.Blue)
                .WithFields(rawAll
                    .Select(p => new EmbedFieldBuilder()
                        .WithName(p.Result)
                        .WithValue(p.Value))).Build());
        }

        [SlashCommand("complete", "Подтверждает выполнение события в расписании")]
        public async Task Complete(
            [Summary("ключ")] string key,
            [Summary("время_окончания")] DateTime endTime,
            [Summary("активный_участник_1")] IUser? activeUser1 = null,
            [Summary("активный_участник_2")] IUser? activeUser2 = null,
            [Summary("активный_участник_3")] IUser? activeUser3 = null)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            var (success, result) = await ScheduleModule.CompleteAsync(
                key,
                endTime,
                activeUser1?.Id,
                activeUser2?.Id,
                activeUser3?.Id);

            if (!success)
            {
                await channel.SendMessageAsync(result);
                return;
            }
            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle(result)
                .WithColor(Color.Green)
                .Build());
        }
    }
}
