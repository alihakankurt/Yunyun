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

            await player.StopAsync();
            await ReplyAsync("Playback stopped.");
        }
    }
}