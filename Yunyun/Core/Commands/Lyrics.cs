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
        [Alias("l")]
        [Summary("Shows the currently playing track's lyrics.")]
        public async Task LyricsCommand([Remainder][Summary("The optional song name")] string song = null)
        {
            var player = LavalinkService.GetPlayer(Context.Guild);
            song ??= (player != null ? player.Track.Title : string.Empty);
            if (song == null)
            {
                await ReplyAsync("No song name provided for search!");
                return;
            }

            var response = await LavalinkService.SearchGeniusAsync(song);
            if (response == null)
            {
                await ReplyAsync("No lyrics could be found!");
                return;
            }

            var embed = new EmbedBuilder()
                .WithFooter(footer =>
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithTitle(response.Title)
                .WithDescription((response.Lyrics.Length > 2048) ? $"{response.Lyrics[..2045]}..." : response.Lyrics)
                .WithUrl(response.Url)
                .WithThumbnailUrl(response.Thumbnail)
                .WithColor(255, 79, 0)
                .WithCurrentTimestamp().Build();
            await ReplyAsync(embed: embed);
        }
    }
}