using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Victoria.Enums;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Search")]
        [Command("search", RunMode = RunMode.Async)]
        [Summary("Searchs in YouTube with given query.")]
        public async Task SearchCommand([Remainder] [Summary("The search query about track or URL.")] string query)
        {
            var search = await LavalinkService.SearchAsync(query);
            var tracks = search.Tracks.Take(10);

            if (search.LoadStatus == LoadStatus.LoadFailed || search.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync("No tracks could be found!");
                return;
            }

            else
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Search Results")
                    .WithFooter(footer =>
                    {
                        footer.Text = $"Invoked by {Context.User}";
                        footer.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithDescription(string.Join("\n", tracks.Select((t, i) => $"`{i + 1})` [{t.Title} ({t.Duration})]({t.Url})")))
                    .WithColor(255, 79, 0)
                    .WithCurrentTimestamp().Build();
                
                await ReplyAsync(embed: embed);
            }
        }
    }
}