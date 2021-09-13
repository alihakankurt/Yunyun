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
        [Name("Advenced Equalizer")]
        [Command("adveq", RunMode = RunMode.Async)]
        [Summary("Advanced version of the equalizer command.")]
        public async Task AdvencedEqualizerCommand([Summary("Must be between 1 and 15.")] int band, [Summary("Gets value between -2.5dB and 10dB.")] double gain)
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

            var equalizer = LavalinkService.GetEqualizer(band, gain);

            if (equalizer is null)
            {
                await ReplyAsync("The equalizer band or gain is out of bounds.");
                return;
            }

            await player.EqualizerAsync(equalizer);
            await ReplyAsync("Equalizer adjusted.");
        }
    }
}
