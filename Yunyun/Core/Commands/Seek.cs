using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Seek")]
        [Command("seek", RunMode = RunMode.Async)]
        [Summary("Seeks the current track.")]
        public async Task SeekCommand([Remainder] [Summary("The timestamp that you want to seek. (hh:mm:ss)")] string timestamp)
        {
            var player = LavalinkService.GetPlayer(Context.Guild);

            if (player is null)
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            if ((Context.User as SocketGuildUser).VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync($"You need to be in `{player.VoiceChannel.Name}` for do that!");
                return;
            }

            if (player.Track is null)
            {
                await ReplyAsync("Nothing is playing right now.");
                return;
            }

            if(!TimeSpan.TryParse(timestamp, out var result))
            {
                await ReplyAsync("Invalid timestamp!");
                return;
            }

            await player.SeekAsync(result);
        }
    }
}