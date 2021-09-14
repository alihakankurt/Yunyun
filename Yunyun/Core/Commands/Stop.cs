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
        [Name("Stop")]
        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Stops the playback.")]
        public async Task StopCommand()
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

            await player.StopAsync();
            await ReplyAsync("Playback stopped.");
        }
    }
}