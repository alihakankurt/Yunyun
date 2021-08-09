using System;
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
        [Name("Resume")]
        [Command("resume", RunMode = RunMode.Async)]
        [Summary("Resumes the playback.")]
        public async Task ResumeCommand()
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

            if (player.PlayerState != PlayerState.Paused)
            {
                await ReplyAsync("Playback is not paused!");
                return;
            }

            await player.ResumeAsync();
            await ReplyAsync("Resumed.");
        }
    }
}