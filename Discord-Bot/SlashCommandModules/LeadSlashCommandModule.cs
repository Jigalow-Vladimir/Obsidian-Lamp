using Discord;
using Discord.Interactions;
using Discord_Bot.Models;
using Discord_Bot.StaticModules;

namespace Discord_Bot.SlashCommandModules
{
    [Group("lead", "Управление информацией о ведущих")]
    public class LeadSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("list", "Вывод всех ведущих")]
        public async Task List()
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            var (success, result) = await LeadModule.GetListAsync();

            if (!success)
            {
                await channel.SendMessageAsync(result[0].ResultOrName);
                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
               .WithTitle("Ведущие")
               .WithColor(Color.Gold)
               .WithFields(result.Select(s => new EmbedFieldBuilder()
                   .WithName($"<@{s.ResultOrName}")
                   .WithValue(s.Value)))
               .Build());
        }

        [SlashCommand("get", "Выводит статистику ведущего")]
        public async Task Get(
            [Summary("ведущий")] IUser? lead = null)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            if (lead == null) lead = Context.User;
            var (success, result) = await LeadModule.GetAsync(lead.Id.ToString());

            if (!success)
            {
                await channel.SendMessageAsync(result);
                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("Ведущий")
                .WithColor(Color.Gold)
                .WithDescription($"<@{lead.Id}>\n" +
                    Lead.FromJson(result).ToString()).Build());
        }

        [RequireRole("Moderator")]
        [SlashCommand("delete", "Удаляет ведущего")]
        public async Task DeleteLead(
            [Summary("ведущий")] IUser lead)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            var (success, result) = await LeadModule.DeleteAsync(lead.Id.ToString());

            if (!success)
            {
                await channel.SendMessageAsync(result);
                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle(result)
                .WithColor(Color.Red).Build());
        }

        [RequireRole("Moderator")]
        [SlashCommand("put", "Добавляет нового ведущего")]
        public async Task Put(
            [Summary("ведущий")] IUser lead)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            var (success, result) = await LeadModule.PutAsync(lead.Id);

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

        [RequireRole("Moderator")]
        [SlashCommand("quota", "Меняет квоту ведущему")]
        public async Task Quota(
            [Summary("ведущий")] IUser lead, 
            [Summary("квота")] uint newQuota)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;

            var (success, result) = await LeadModule.GetAsync(lead.Id.ToString());

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

        [RequireRole("Moderator")]
        [SlashCommand("new-month", "Устанавливает количество игр за этот месяц в ноль у всех ведущих")]
        public async Task NewMonth()
        {
            await RespondAsync("Process...", ephemeral: true);
            var channel = Context.Channel as ITextChannel;
            if (channel == null)
                return;
            
            var (success, result) = await LeadModule.NewMonthAsync();
            if (!success)
            {
                await channel.SendMessageAsync(result);
                return;
            }
            
            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle(result)
                .WithColor(Color.Green).Build());
        }
    }
}