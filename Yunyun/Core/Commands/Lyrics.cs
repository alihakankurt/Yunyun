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
        [Name("Lyrics")]
        [Command("lyrics", RunMode = RunMode.Async)]
        [Summary("Shows the currently playing track's lyrics.")]
        public async Task LyricsCommand()
        {
            var player = LavalinkService.GetPlayer(Context.Guild);
            
            if (player is null)
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            else if (player.Track is null)
            {
                await ReplyAsync("Nothing is playing right now!");
                return;
            }
            
            var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
            lyrics = lyrics.Length > 2000 ? $"{lyrics.Substring(0, 1997)}..." : lyrics;
            lyrics = string.IsNullOrEmpty(lyrics) ? "No lyrics could be found." : lyrics;
                
            var embed = new EmbedBuilder()
                .WithTitle($"{player.Track.Title}")
                .WithColor(255, 79, 0)
                .WithDescription(lyrics)
                .WithThumbnailUrl(await player.Track.FetchArtworkAsync())
                .WithFooter(footer => 
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithCurrentTimestamp().Build();
            
            await ReplyAsync(embed: embed);
        }
    }
}