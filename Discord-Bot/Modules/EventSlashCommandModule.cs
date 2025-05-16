using Discord;
using Discord.Interactions;
using Discord_Bot.Models;
using System.Text.Json;

namespace Discord_Bot.Modules
{
    public class EventSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CloudflareApiHandler _api = new(Resources.Credentials["cloudflare-namespace-events"]);
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        [SlashCommand("event-get", "������� ������� �� �����")]
        public async Task Get(
            [Summary("����")] string key)
        {
            await RespondAsync("Process...", ephemeral: true);

            var embedBuilder = new EmbedBuilder()
                .WithTitle("�������")
                .WithColor(Color.Blue)
                .WithDescription(Part
                    .PartFromJson(await _api
                        .GetAsync(key)).ToString());

            var channel = Context.Channel as ITextChannel;
            if (channel != null)
                await channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        [SlashCommand("event-put", "post an event")]
        public async Task Put(
            [Summary("�������")] string name,
            [Summary("�������")] IUser lead,
            [Summary("����", "������������� �� ����� �������: `dd.mm.yyyy hh:mm`")] DateTime date,
            [Summary("��������_��������_1")] IUser? user1 = null,
            [Summary("��������_��������_2")] IUser? user2 = null,
            [Summary("��������_��������_3")] IUser? user3 = null)
        {
            await RespondAsync("Process...", ephemeral: true);

            var part = new Part(
                name,
                lead.Id,
                date,
                user1 == null ? 0 : user1.Id,
                user2 == null ? 0 : user2.Id,
                user3 == null ? 0 : user3.Id);

            await _api.PutAsync(part.Key, JsonSerializer.Serialize(part, _jsonOptions));

            var channel = Context.Channel as ITextChannel;
            if (channel != null)
                await channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("Put: Success")
                    .WithColor(Color.Green)
                    .WithDescription(part.ToString()).Build());
        }

        [SlashCommand("event-delete", "delete an event")]
        public async Task Delete(
            [Summary("����")] string key)
        {
            await RespondAsync("Process...", ephemeral: true);

            await _api.DeleteAsync(key);

            var channel = Context.Channel as ITextChannel;
            if (channel != null)
                await channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("Delete: Success")
                    .WithColor(Color.Red).Build());
        }

        [SlashCommand("event-list", "������� ��������� ���-�� �������")]
        public async Task List(
            [Summary("����������", "�������� 25")]
            [MaxValue(EmbedBuilder.MaxFieldCount)]
            [MinValue(1)]
            uint count = 1)
        {
            await RespondAsync("Process...", ephemeral: true);
            
            var keys = JsonSerializer
                .Deserialize<List<ResultItem>>(JsonDocument
                    .Parse(await _api.GetAllAsync()).RootElement
                        .GetProperty("result"), _jsonOptions);

            if (keys == null)
                return;

            List<Part> parts = new();
            for (int i = 0; i < count && i < keys.Count; i++)
            {
                var part = JsonSerializer.Deserialize<Part>(
                    await _api.GetAsync(keys[i].Name), _jsonOptions);

                if (part == null)
                    continue;

                parts.Add(part);
            }

            var channel = Context.Channel as ITextChannel;
            if (channel != null)
                await channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("�������")
                    .WithColor(Color.Blue)
                    .WithFields(parts
                        .Select(p => new EmbedFieldBuilder()
                            .WithName(p.Key)
                            .WithValue(p.ToString()))).Build());
        }
    }
}