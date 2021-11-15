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
        [Name("Shuffle")]
        [Command("shuffle", RunMode = RunMode.Async)]
        [Summary("Shuffles the queue.")]
        public async Task ShuffleCommand()
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

            player.Queue.Shuffle();
            await ReplyAsync("Queue shuffled.");
        }
    }
}