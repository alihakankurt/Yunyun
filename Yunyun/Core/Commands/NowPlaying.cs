using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Victoria;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Now Playing")]
        [Command("nowplaying", RunMode = RunMode.Async)]
        [Alias("np")]
        [Summary("Shows the currently playing track.")]
        public async Task NowPlayingCommand()
        {
            var player = LavalinkService.GetPlayer(Context.Guild);

            if (player is null)
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("🎶Now Playing")
                .WithColor(255, 79, 0)
                .WithDescription(player.Track is null ? "Nothing" : player.Track.GetTimeline())
                .WithThumbnailUrl(player.Track is null ? "" : await player.Track.FetchArtworkAsync())
                .WithCurrentTimestamp().Build();

            await ReplyAsync(embed: embed);
        }
    }
}