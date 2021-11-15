using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Stats")]
        [Command("stats", RunMode = RunMode.Async)]
        [Summary("Shows the stats of player and bot.")]
        public async Task StatsCommand()
        {
            var process = Process.GetCurrentProcess();
            var player = LavalinkService.GetPlayer(Context.Guild);

            var embed = new EmbedBuilder()
                .WithTitle("Stats")
                .WithAuthor(author =>
                {
                    author.Name = Context.Client.CurrentUser.ToString();
                    author.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                })
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .AddField("Latency", $"{Context.Client.Latency} ms", true)
                .AddField(".NET Version", "Core 3.1", true)
                .AddField("C# Version", "9.0", true)
                .AddField("Bot Version", ConfigurationService.Version, true)
                .AddField("Discord.NET Version", "2.4.0", true)
                .AddField("Player State", player is null ? "Not connected" : $"Voice Channel: <#{player.VoiceChannel.Id}>\nText Channel: {player.TextChannel.Mention}\nState: {player.PlayerState}\nVolume: {player.Volume}", false)
                .AddField("RAM Usage", $"{process.PrivateMemorySize64 / 1048576} MB", true)
                .AddField("CPU Time", $"{process.TotalProcessorTime.TotalMilliseconds} ms", true)
                .WithColor(255, 79, 0)
                .WithCurrentTimestamp().Build();

            await ReplyAsync(embed: embed);
        }
    }
}