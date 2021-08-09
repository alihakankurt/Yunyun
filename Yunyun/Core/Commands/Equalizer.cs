using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yunyun.Core.Services;
using Discord.Commands;
using Discord.WebSocket;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Equalizer")]
        [Command("equalizer", RunMode = RunMode.Async)]
        [Alias("eq")]
        [Summary("Sets the player's equalizer.")]
        public async Task EqualizerCommand([Remainder] [Summary("The name of the equalizer preset.")] string preset)
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

            var equalizer = LavalinkService.GetEqualizer(preset);

            if (equalizer is null)
            {
                await ReplyAsync("Invalid equalizer preset. Try one of these: `flat`, `boost`, `piano`, `metal`");
                return;
            }

            await player.EqualizerAsync(equalizer);
            await ReplyAsync("Equalizer adjusted.");
        }
    }
}
