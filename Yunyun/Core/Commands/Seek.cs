using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Seek")]
        [Command("seek", RunMode = RunMode.Async)]
        [Summary("Seeks the current track.")]
        public async Task SeekCommand([Remainder] [Summary("The timestamp that you want to seek. (mm:ss)")] string timestamp)
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

            if (player.Track is null)
            {
                await ReplyAsync("Nothing is playing right now.");
                return;
            }

            var match = Regex.Match(timestamp, "^([0-9]{1,2})[:.]?([0-9]{1,2})?$");
            
            if (!match.Success)
            {
                await ReplyAsync("Invalid timestamp!");
                return;
            }

            var result = TimeSpan.FromSeconds(match.Groups.Values.ElementAt(2).Success
                ? Convert.ToInt32(match.Groups.Values.ElementAt(1).Value) * 60 + Convert.ToInt32(match.Groups.Values.ElementAt(2).Value)
                : Convert.ToInt32(match.Groups.Values.ElementAt(1).Value));

            if (player.Track.Duration.CompareTo(result) < 1)
            {
                await ReplyAsync("Timestamp can't exceed the current track's duration!");
                return;
            }

            await player.SeekAsync(result);
        }
    }
}