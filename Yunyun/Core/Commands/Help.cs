using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Yunyun.Core.Extensions;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Help")]
        [Command("help")]
        [Summary("Shows this message.")]
        public async Task HelpCommand([Remainder] [Summary("That you want to see info.")] string command = null)
        {
            if (command == null)
            {
                var embed = new EmbedBuilder()
                    .WithTitle($"{Context.Client.CurrentUser.Username}'s Commands")
                    .WithDescription(string.Join("\n", ProviderService.GetService<CommandService>().Commands.Select(c => $"`{ConfigurationService.Prefix}{string.Join("|", c.Aliases)}` **-->** {c.Summary}")))
                    .WithColor(255, 79, 0)
                    .WithFooter(footer => 
                    {
                        footer.Text = $"Requested by {Context.User}";
                        footer.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithCurrentTimestamp().Build();
                await ReplyAsync(embed: embed);
            }
        
            else
            {
                var cmd = ProviderService.GetService<CommandService>().Commands.Where(c => c.Aliases.Contains(command.ToLower())).First();
                if (cmd == null)
                {
                    await ReplyAsync("This command does not exists!");
                }

                else
                {
                    var embed = new EmbedBuilder()
                        .WithTitle($"{cmd.Name}")
                        .AddField("Syntax", cmd.GetSyntax(), false)
                        .AddField("Summary", cmd.Summary, false)
                        .AddField("Parameters", cmd.Parameters.Count() > 0 ? string.Join("\n", cmd.Parameters.Select(p => $"{(p.IsOptional ? $"`[{p}]`" : $"`<{p}>`")}**:** {p.Summary}")) : "None", false)
                        .WithColor(255, 79, 0)
                        .WithCurrentTimestamp().Build();

                    await ReplyAsync(embed: embed);
                }
            }
        }
    }
}