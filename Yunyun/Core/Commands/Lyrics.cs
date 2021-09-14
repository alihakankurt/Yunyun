using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Victoria;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Lyrics")]
        [Command("lyrics", RunMode = RunMode.Async)]
        [Summary("Shows the currently playing track's lyrics.")]
        public async Task LyricsCommand([Remainder] [Summary("Optional song name.")] string song = null)
        {
            song = song ?? LavalinkService.GetPlayer(Context.Guild)?.Track?.Title;
            
            if (string.IsNullOrWhiteSpace(song))
            {
                await ReplyAsync("No song name provided for search.");
                return;
            }

            try
            {
                var lyrics = await LavalinkService.GetLyricsAsync(song);
                lyrics.Lyrics = lyrics.Lyrics.Length > 2000 ? $"{lyrics.Lyrics.Substring(0, 1997)}..." : lyrics.Lyrics;

                var embed = new EmbedBuilder()
                    .WithTitle($"{lyrics.Title} - {lyrics.Author}")
                    .WithColor(255, 79, 0)
                    .WithDescription(lyrics.Lyrics)
                    .WithThumbnailUrl(lyrics.Thumbnail)
                    .WithUrl(lyrics.Links)
                    .WithFooter(footer =>
                    {
                        footer.Text = $"Requested by {Context.User}";
                        footer.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithCurrentTimestamp().Build();

                await ReplyAsync(embed: embed);
            }
            
            catch (HttpRequestException)
            {
                await ReplyAsync("No lyrics could be found!");
            }
        }
    }
}