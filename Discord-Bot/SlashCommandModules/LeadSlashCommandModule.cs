using Discord;
using Discord.Interactions;
using Discord_Bot.Models;
using Discord_Bot.StaticModules;

namespace Discord_Bot.SlashCommandModules
{
    [Group("lead", "manage lead info")]
    public class LeadSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private async Task<ITextChannel?> GetTextChannelAsync()
        {
            await RespondAsync(Consts.ProcessingMessage, ephemeral: true);

            return Context.Channel as ITextChannel;
        }

        private static async Task RespondEmbedAsync(
            ITextChannel channel, 
            string title, 
            Color color, 
            string? description = null, 
            IEnumerable<EmbedFieldBuilder>? fields = null)
        {
            var builder = new EmbedBuilder().WithTitle(title).WithColor(color);
            
            if (description != null)
                builder.WithDescription(description);
            
            if (fields != null)
                builder.WithFields(fields);
            
            await channel.SendMessageAsync(embed: builder.Build());
        }

        private static async Task RespondTextOrEmbedAsync(
            ITextChannel channel, 
            bool success, 
            string message, 
            Color successColor, 
            string? embedDescription = null)
        {
            if (!success)
                await channel.SendMessageAsync(message);
            else
                await RespondEmbedAsync(channel, message, successColor, embedDescription);
        }

        [SlashCommand("list", "show all leads")]
        public async Task List()
        {
            var channel = await GetTextChannelAsync();

            if (channel == null) return;

            var (success, result) = await LeadModule.GetListAsync();
            if (!success)
            {
                await channel.SendMessageAsync(result[0].ResultOrName);
            
                return;
            }

            var fields = result.Select(s => new EmbedFieldBuilder()
                .WithName(s.ResultOrName)
                .WithValue(s.Value));

            await RespondEmbedAsync(channel, "leads", Consts.InfoColor, fields: fields);
        }

        [SlashCommand("get", "show lead stats")]
        public async Task Get([Summary("lead")] IUser? lead = null)
        {
            var channel = await GetTextChannelAsync();

            if (channel == null) return;

            lead ??= Context.User;

            var (success, result) = await LeadModule.GetAsync(lead.Id.ToString());

            if (!success)
            {
                await channel.SendMessageAsync(result);
                return;
            }

            var leadObj = Lead.FromJson(result);

            await RespondEmbedAsync(channel, "Lead", Consts.InfoColor, leadObj.ToString());
        }

        [RequireRole("Moderator")]
        [SlashCommand("delete", "remove lead")]
        public async Task DeleteLead([Summary("lead")] IUser lead)
        {
            var channel = await GetTextChannelAsync();

            if (channel == null) return;

            var (success, result) = await LeadModule.DeleteAsync(lead.Id.ToString());

            await RespondTextOrEmbedAsync(channel, success, result, Consts.ErrorColor);
        }

        [RequireRole("Moderator")]
        [SlashCommand("put", "add new lead")]
        public async Task Put([Summary("lead")] IUser lead)
        {
            var channel = await GetTextChannelAsync();

            if (channel == null) return;

            var (success, result) = await LeadModule.PutAsync(lead.Id);

            await RespondTextOrEmbedAsync(channel, success, result, Consts.SuccessColor);
        }

        [RequireRole("Moderator")]
        [SlashCommand("quota", "change lead quota")]
        public async Task Quota([Summary("lead")] IUser lead, [Summary("quota")] uint newQuota)
        {
            var channel = await GetTextChannelAsync();

            if (channel == null) return;

            var (success, result) = await LeadModule.QuotaAsync(lead.Id.ToString(), newQuota);

            await RespondTextOrEmbedAsync(channel, success, result, Consts.SuccessColor);
        }

        [RequireRole("Moderator")]
        [SlashCommand("new-month", "reset monthly games for all leads")]
        public async Task NewMonth()
        {
            var channel = await GetTextChannelAsync();

            if (channel == null) return;

            var (success, result) = await LeadModule.NewMonthAsync();

            await RespondTextOrEmbedAsync(channel, success, result, Consts.SuccessColor);
        }
    }
}
