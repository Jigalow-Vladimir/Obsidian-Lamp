using Discord;
using Discord.Interactions;
using Discord_Bot.StaticModules;
using System.Globalization;

namespace Discord_Bot.SlashCommandModules
{
    [Group("schedule", "controls the schedule")]
    public class ScheduleSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private ITextChannel? GetTextChannel() => Context.Channel as ITextChannel;

        [SlashCommand("get", "print an event in schedule by key")]
        public async Task Get([Summary("key")] string key)
        {
            await RespondAsync("process...", ephemeral: true);

            var channel = GetTextChannel();

            if (channel == null) return;

            var (success, result) = await ScheduleModule.GetAsync(key);

            if (!success)
            {
                await RespondAsync(result);
                return;
            }

            await channel.SendMessageAsync(success ? $"get: {result}" : $"error getting data: {result}");
        }

        [SlashCommand("put", "set an event in schedule")]
        public async Task Put(
            [Summary("event")] string name,
            [Summary("lead_1")] IUser lead1,
            [Summary("date_of_start", $"template: {Consts.DateFormat}")] string datestr,
            [Summary("lead_2")] IUser? lead2 = null,
            [Summary("lead_3")] IUser? lead3 = null)
        {
            await RespondAsync("process...", ephemeral: true);

            var channel = GetTextChannel();

            if (channel == null) return;

            if (!DateTime.TryParseExact(
                datestr,
                Consts.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date))
            {
                await channel.SendMessageAsync(Consts.DateFormatError);

                return;
            }

            var (success, result) = await ScheduleModule
                .PutAsync(name, lead1.Id, date, lead2?.Id, lead3?.Id);

            var embed = new EmbedBuilder()
                .WithTitle(result)
                .WithColor(success ? Consts.SuccessColor : Consts.ErrorColor)
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        [SlashCommand("delete", "removes an event from the schedule")]
        public async Task Delete([Summary("key")] string key)
        {
            await RespondAsync("process...", ephemeral: true);

            var channel = GetTextChannel();

            if (channel == null) return;

            var (success, result) = await ScheduleModule.DeleteAsync(key);

            var embed = new EmbedBuilder().WithTitle(result).WithColor(success ? Consts.SuccessColor : Consts.ErrorColor).Build();

            await channel.SendMessageAsync(embed: embed);
        }

        [SlashCommand("list", "displays events in the schedule")]
        public async Task List(
            [Summary("count", "min: 1, max: 25")]
            [MaxValue(EmbedBuilder.MaxFieldCount)]
            [MinValue(1)]
            uint count = 1)
        {
            await RespondAsync("process...", ephemeral: true);

            var channel = GetTextChannel();

            if (channel == null) return;

            var (success, data) = await ScheduleModule.ListAsync(count);

            if (!success)
            {
                await channel.SendMessageAsync($"error getting list of events: {data[0].Result}");
            
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("events")
                .WithColor(Consts.InfoColor)
                .WithFields(data.Select(p => new EmbedFieldBuilder().WithName(p.Result).WithValue(p.Value)))
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        [SlashCommand("list-pretty", "displays events in the schedule")]
        public async Task ListPretty(
            [Summary("count", "min: 1, max: 25")]
            [MaxValue(EmbedBuilder.MaxFieldCount)]
            [MinValue(1)]
            uint count = 1)
        {
            await RespondAsync("process...", ephemeral: true);

            var channel = GetTextChannel();

            if (channel == null) return;

            var (success, data) = await ScheduleModule.ListAsync(count, true);

            if (!success)
            {
                await channel.SendMessageAsync($"error getting list of events: {data[0].Result}");
                return;
            }

            var result = string.Join("\n", data.Select(p => p.Value));
            
            var embed = new EmbedBuilder()
                .WithTitle("events")
                .WithColor(Consts.InfoColor)
                .WithDescription(result)
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        [SlashCommand("complete", "complete an event in schedule")]
        public async Task Complete(
            [Summary("key")] string key,
            [Summary("end_date", $"template: {Consts.DateFormat}")] string endDateStr,
            [Summary("active_user_1")] IUser? activeUser1 = null,
            [Summary("active_user_2")] IUser? activeUser2 = null,
            [Summary("active_user_3")] IUser? activeUser3 = null)
        {
            await RespondAsync("process...", ephemeral: true);

            var channel = GetTextChannel();

            if (channel == null) return;

            if (!DateTime.TryParseExact(endDateStr, Consts.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                await channel.SendMessageAsync(Consts.DateFormatError);
                return;
            }

            var (success, result) = await ScheduleModule.CompleteAsync(
                key, endDate, activeUser1?.Id, activeUser2?.Id, activeUser3?.Id);

            var embed = new EmbedBuilder()
                .WithTitle(result)
                .WithColor(success ? Consts.SuccessColor : Consts.ErrorColor)
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
    }
}
