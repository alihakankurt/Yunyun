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
        [Name("Pause")]
        [Command("pause", RunMode = RunMode.Async)]
        [Summary("Pauses the playback.")]
        public async Task PauseCommand()
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

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("Playback is not playing!");
                return;
            }

            await player.PauseAsync();
            await ReplyAsync("Paused.");
        }
    }
}