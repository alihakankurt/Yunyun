using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Yunyun.Core.Services;

namespace Yunyun.Core.Commands
{
    public partial class YunCommandModule : ModuleBase<SocketCommandContext>
    {
        [Name("Set Prefix")]
        [Command("setprefix", RunMode = RunMode.Async)]
        [Summary("Sets the bots prefix (This command can only be used by owner).")]
        [RequireOwner]
        public async Task SetPrefixCommand([Summary("The new prefix.")] string prefix)
        {
            ConfigurationService.Prefix = prefix;
            ConfigurationService.SaveConfigFile();
            await ReplyAsync($"Prefix has been setted to {prefix}");
        }
    }
}
