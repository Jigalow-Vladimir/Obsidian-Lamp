using Discord;
using Discord.Interactions;
using Discord_Bot.Models;
using Discord_Bot.StaticModules;
using System.Text.Json;

namespace Discord_Bot.SlashCommandModules
{
    [Group("event", "���������� ����������� � ���� ��������")]
    public class EventSlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("get", "������� ������� �� �����")]
        public async Task Get([Summary("����")] string key)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null) return;

            var (success, result) = await EventModule.GetAsync(key);

            if (!success)
            {
                await channel.SendMessageAsync(result);
                return;
            }

            var part = Part.PartFromJson(result);
            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("�������")
                .WithColor(Color.Blue)
                .WithDescription(part.ToString()).Build());
        }

        [RequireRole("Moderator")]
        [SlashCommand("put", "������������� �������")]
        public async Task Put(
            [Summary("�������")] string name,
            [Summary("�������")] IUser lead,
            [Summary("����_������", "������: `dd.mm.yy hh:mm` -> 17.10.25 16:00 :")] DateTime startDate,
            [Summary("����_�����", "������: `dd.mm.yy hh:mm` -> 17.10.25 17:00")] DateTime endDate,
            [Summary("��������_��������_1")] IUser? user1 = null,
            [Summary("��������_��������_2")] IUser? user2 = null,
            [Summary("��������_��������_3")] IUser? user3 = null)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null) return;

            var (success, result) = await EventModule.PutAsync(name, lead.Id, startDate, endDate,
                user1?.Id, user2?.Id, user3?.Id);

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

        [SlashCommand("delete", "������� �������")]
        public async Task Delete([Summary("����")] string key)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null) return;

            var (success, result) = await EventModule.DeleteAsync(key);

            if (!success)
            {
                await channel.SendMessageAsync(result);
                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle(result)
                .WithColor(Color.Red).Build());
        }

        [SlashCommand("list", "������� ��������� ���-�� �������")]
        public async Task List(
           [Summary("����������", "�������� 25")]
           [MaxValue(EmbedBuilder.MaxFieldCount)]
           [MinValue(1)]
           uint count = 1)
        {
            await RespondAsync("Process...", ephemeral: true);

            var channel = Context.Channel as ITextChannel;
            if (channel == null) return;

            var (success, rawAll) = await EventModule.GetListAsync(count);

            if (!success || rawAll == null)
            {
                await channel.SendMessageAsync(rawAll != null ? rawAll[0].Item1 : "Error");
                return;
            }

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("�������")
                .WithColor(Color.Blue)
                .WithFields(rawAll
                    .Select(p => new EmbedFieldBuilder()
                        .WithName(p.Item1)
                        .WithValue(p.Item2)))
                .Build());
        }
    }
}