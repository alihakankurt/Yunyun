using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria.Enums;
using Victoria.Responses.Search;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Play")]
        [Command("play", RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("Plays a track in voice channel.")]
        public async Task PlayCommand([Remainder] [Summary("The search query about track or URL.")] string query)
        {
            var player = LavalinkService.GetPlayer(Context.Guild);

            if (player is null)
            {
                await JoinCommand();
                player = LavalinkService.GetPlayer(Context.Guild);
            }

            var channel = (Context.User as SocketGuildUser).VoiceChannel;

            if (channel != player.VoiceChannel)
            {
                if (channel == null || (await player.VoiceChannel.GetUsersAsync().FlattenAsync()).Where(x => !x.IsBot).Count() > 0)
                {
                    await ReplyAsync($"You need to be in `{player.VoiceChannel.Name}` for do that!");
                    return;
                }

                await LavalinkService.MoveAsync(channel);
            }

            var search = await LavalinkService.SearchAsync(query);

            if (search.Status == SearchStatus.LoadFailed || search.Status == SearchStatus.NoMatches)
            {
                await ReplyAsync("No tracks could be found!");
                return;
            }

            else if (search.Status == SearchStatus.PlaylistLoaded)
            {
                if (player.Track is null && player.PlayerState != PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    var track = search.Tracks.First();
                    await player.PlayAsync(track);
                    player.Queue.Enqueue(search.Tracks.TakeLast(search.Tracks.Count - 1));
                }
                
                else
                {
                    player.Queue.Enqueue(search.Tracks);
                }
                
                await ReplyAsync($"{search.Tracks.Count()} tracks added to queue.");
            }

            else
            {
                var track = search.Tracks.First();

                if (player.Track is null && player.PlayerState != PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    await player.PlayAsync(track);
                }

                else
                {
                    player.Queue.Enqueue(track);
                }

                await ReplyAsync($"`{track.Title} - {track.Author}` added to queue.");
            }
        }
    }
}