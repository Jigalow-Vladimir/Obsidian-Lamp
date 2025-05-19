using Discord;
using Discord.Interactions;
using Discord_Bot.Models;
using Discord_Bot.StaticModules;
using System.Globalization;

namespace Discord_Bot.SlashCommandModules
{
    [Group("event", "controls events info")]
    public class EventSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("get", "print event by key")]
        public async Task Get([Summary("key")] string key)
        {
            await RespondAsync(Consts.ProcessingMessage, ephemeral: true);

            if (Context.Channel is not ITextChannel channel) return;

            var (success, result) = await EventModule.GetAsync(key);
            if (!success)
            {
                await channel.SendMessageAsync(result);
                return;
            }

            var part = Part.PartFromJson(result);
            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("event")
                .WithColor(Consts.InfoColor)
                .WithDescription(part.ToString())
                .Build());
        }

        [RequireRole("Moderator")]
        [SlashCommand("put", "set new event")]
        public async Task Put(
            [Summary("event")] string name,
            [Summary("lead_1")] IUser lead1,
            [Summary("start", "format: `dd.MM.yy HH:mm`")] string startDateStr,
            [Summary("end", "format: `dd.MM.yy HH:mm`")] string endDateStr,
            [Summary("lead_2")] IUser? lead2 = null,
            [Summary("lead_3")] IUser? lead3 = null,
            [Summary("user_1")] IUser? user1 = null,
            [Summary("user_2")] IUser? user2 = null,
            [Summary("user_3")] IUser? user3 = null)
        {
            await RespondAsync(Consts.ProcessingMessage, ephemeral: true);

            if (Context.Channel is not ITextChannel channel) return;

            if (!DateTime.TryParseExact(startDateStr, Consts.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParseExact(endDateStr, Consts.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                await channel.SendMessageAsync(Consts.DateFormatError);

                return;
            }

            var (success, result) = await EventModule.PutAsync(
                name, lead1.Id, startDate, endDate,
                lead2?.Id, lead3?.Id, user1?.Id, user2?.Id, user3?.Id);

            if (!success)
            {
                await channel.SendMessageAsync(result);

                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle(result)
                .WithColor(Consts.SuccessColor)
                .Build());
        }

        [SlashCommand("delete", "remove event")]
        public async Task Delete([Summary("key")] string key)
        {
            await RespondAsync(Consts.ProcessingMessage, ephemeral: true);

            if (Context.Channel is not ITextChannel channel) return;

            var (success, result) = await EventModule.DeleteAsync(key);

            if (!success)
            {
                await channel.SendMessageAsync(result);

                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle(result)
                .WithColor(Consts.ErrorColor)
                .Build());
        }

        [SlashCommand("list", "show events")]
        public async Task List(
            [Summary("count", "1-25")]
            [MaxValue(EmbedBuilder.MaxFieldCount)]
            [MinValue(1)]
            uint count = 1)
        {
            await RespondAsync(Consts.ProcessingMessage, ephemeral: true);

            if (Context.Channel is not ITextChannel channel) return;

            var (success, rawAll) = await EventModule.GetListAsync(count);

            if (!success || rawAll == null)
            {
                await channel.SendMessageAsync(rawAll?.FirstOrDefault().Item1 ?? "error");
                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("events")
                .WithColor(Consts.InfoColor)
                .WithFields(rawAll.Select(p => new EmbedFieldBuilder()
                    .WithName($"__{p.Item1}__")
                    .WithValue(p.Item2)))
                .Build());
        }
    }
}
