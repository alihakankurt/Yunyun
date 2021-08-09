using System;
using System.Collections.Generic;
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
        [Name("Play")]
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Plays a track in voice channel.")]
        public async Task PlayCommand([Remainder] [Summary("The search query about track or URL.")] string query)
        {
            var player = LavalinkService.GetPlayer(Context.Guild);

            if (player is null)
            {
                await JoinCommand();
                player = LavalinkService.GetPlayer(Context.Guild);
            }
            
            if ((Context.User as SocketGuildUser).VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync($"You need to be in `{player.VoiceChannel.Name}` for do that!");
                return;
            }

            var search = await LavalinkService.GetSearchResponseAsync(query);

            if (search.LoadStatus == LoadStatus.LoadFailed || search.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync("No tracks could be found!");
                return;
            }

            else if (search.LoadStatus == LoadStatus.PlaylistLoaded)
            {
                if (player.Track is null && player.PlayerState != PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    var track = search.Tracks.First();
                    await player.PlayAsync(track);
                    foreach (var t in search.Tracks.Where((t, i) => i != 0))
                        player.Queue.Enqueue(t);
                }
                
                else
                {
                    foreach (var t in search.Tracks)
                        player.Queue.Enqueue(t);
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