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
        [Name("Remove")]
        [Command("remove", RunMode = RunMode.Async)]
        [Alias("rm")]
        [Summary("Removes a track from queue.")]
        public async Task RemoveCommand([Summary("Track's position that want to remove.")] uint position)
        {
            var player = LavalinkService.GetPlayer(Context.Guild);

            if (player is null)
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
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

            if (player.Queue.Count < position)
            {
                await ReplyAsync("No track found in that position.");
                return;
            }

            var track = player.Queue.RemoveAt((int)position - 1);
            await ReplyAsync($"`{track.Title}` removed from queue.");
        }
    }
}