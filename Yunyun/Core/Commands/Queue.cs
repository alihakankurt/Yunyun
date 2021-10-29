using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Queue")]
        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("Shows the queue.")]
        public async Task QueueCommand([Remainder] [Summary("The page that you want to see.")] ushort page = 1)
        {
            var player = LavalinkService.GetPlayer(Context.Guild);
            
            if (player is null)
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var embed = new EmbedBuilder()
                .WithDescription(string.Join("\n", player.Queue.Where((t, i) => i >= page * 10 - 10 && i < page * 10).Select((t, i) => $"`{i + 1})` {t.Title} - {t.Author}")))
                .WithColor(255, 79, 0)
                .WithFooter(footer => 
                {
                    footer.Text = $"Requested by {Context.User}";
                    footer.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithCurrentTimestamp().Build();
            
            await ReplyAsync(embed: embed);
        }
    }
}