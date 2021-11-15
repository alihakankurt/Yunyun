using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria.Enums;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Play Skip")]
        [Command("playskip", RunMode = RunMode.Async)]
        [Alias("ps")]
        [Summary("Skips the current track, adds the track(s) and plays next.")]
        public async Task PlaySkipCommand([Remainder][Summary("The search query about track or URL.")] string query)
        {
            var player = LavalinkService.GetPlayer(Context.Guild);

            if (player == null)
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            if ((Context.User as SocketGuildUser).VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync($"You need to be in `{player.VoiceChannel.Name}` for do that!");
                return;
            }

            var search = await LavalinkService.SearchAsync(query);

            if (player.Track == null && player.PlayerState != PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                await ReplyAsync("Playback is not playing!");
                return;
            }

            else if (search.LoadStatus == LoadStatus.LoadFailed || search.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync("No tracks could be found!");
                return;
            }

            else if (search.LoadStatus == LoadStatus.PlaylistLoaded)
            {
                var track = search.Tracks.First();
                await player.PlayAsync(track);
                foreach (var t in search.Tracks.TakeLast(search.Tracks.Count - 1))
                    player.Queue.Enqueue(t);

                await ReplyAsync($"{search.Tracks.Count()} tracks added to queue.");
            }

            else
            {
                var track = search.Tracks.First();
                player.Queue.Enqueue(track);

                await ReplyAsync($"**{track.Title} - {track.Author}** added to queue.");
            }

            await player.SkipAsync();
        }
    }
}