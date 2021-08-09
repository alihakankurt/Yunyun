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
                .WithDescription(player.Track is null ? "Nothing" : GetTimeline(player.Track))
                .WithThumbnailUrl(player.Track is null ? "" : await player.Track.FetchArtworkAsync())
                .WithCurrentTimestamp().Build();
            
            await ReplyAsync(embed: embed);
        }
        
        private string GetTimeline(LavaTrack track)
        { 
            string timeline = "";
            if (track.IsStream)
            {
                timeline = "`🔴 LIVE `";
            }
            
            else
            {
                int pos = (int)(track.Position.TotalSeconds / track.Duration.TotalSeconds * 20);
            
                for (int i = 0; i < 20; i++)
                {
                    if (i == pos)
                        timeline += "⚪";

                    else
                        timeline += "─";
                }
                timeline += $"\n[{track.Position.ToString(@"hh\:mm\:ss")} / {track.Duration.ToString(@"hh\:mm\:ss")}]";
            }
            
            return $"**[{track.Title} - {track.Author}]({track.Url})**\n{timeline}";
        }
    }
}