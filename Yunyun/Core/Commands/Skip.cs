using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Skip")]
        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skips the current track.")]
        public async Task SkipCommand()
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

            if (player.Queue.Count < 1)
            {
                await ReplyAsync("There is no next track for skip!");
                return;
            }

            await player.SkipAsync();
            await ReplyAsync("Skipped.");
        }
    }
}