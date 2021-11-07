using System;
using System.Net.Http;
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
        [Alias("l")]
        [Summary("Shows the currently playing track's lyrics.")]
        public async Task LyricsCommand([Remainder] [Summary("Optional song name.")] string song = null)
        {
            if (string.IsNullOrWhiteSpace(song))
            {
                var player = LavalinkService.GetPlayer(Context.Guild);
                if (player == null || player?.Track == null)
                {
                    await ReplyAsync("No song name provided for search.");
                    return;
                }

                string lyrics = await player.Track.FetchLyricsFromGeniusAsync();
                if (string.IsNullOrWhiteSpace(lyrics))
                {
                    await LyricsCommand(player.Track.Title);
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithTitle($"{player.Track.Title} - {player.Track.Author}")
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

            else
            {
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
}