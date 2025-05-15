using Discord.Interactions;

namespace Discord_Bot.Modules
{
    public class DefaultSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("echo", "Echo an input")]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }

        [SlashCommand("time", "Echo current time")]
        public async Task Time()
        {
            await RespondAsync(DateTime.Now.ToString());
        }
    }
}