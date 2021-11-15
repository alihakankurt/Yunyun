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
        [Name("Join")]
        [Command("join", RunMode = RunMode.Async)]
        [Summary("Joins to a voice channel.")]
        public async Task JoinCommand([Remainder] [Summary("Target voice channel")] SocketVoiceChannel channel = null)
        {
            var player = LavalinkService.GetPlayer(Context.Guild);
            var target = channel ?? (Context.User as IVoiceState).VoiceChannel;
            if (target == null)
            {
                await ReplyAsync("No voice channel is provided!");
                return;
            }
            
            if (player != null && channel == null)
            {
                await ReplyAsync("I'm already connected to voice channel!");
                return;
            }

            else if (player != null && channel == null)
            {
                await LavalinkService.MoveAsync(channel);
            }
            
            else
            {
                await LavalinkService.JoinAsync(target, Context.Channel as ITextChannel);
                player = LavalinkService.GetPlayer(Context.Guild);
                await player.UpdateVolumeAsync(70);
            }
            
            await ReplyAsync($"Joined to `{target.Name}` and bound to {(Context.Channel as SocketTextChannel).Mention}.");
        }
    }
}