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
        [Name("Volume")]
        [Command("volume", RunMode = RunMode.Async)]
        [Alias("vol")]
        [Summary("Sets the volume of playback.")]
        public async Task VolumeCommand([Remainder] [Summary("An integer between 1 and 150.")] ushort volume)
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

            if (volume > 150 || volume < 1)
            {
                await ReplyAsync("Volume must be between 1 and 150!");
                return;
            }

            await player.UpdateVolumeAsync(volume);
            await ReplyAsync("Volume changed.");
        }
    }
}