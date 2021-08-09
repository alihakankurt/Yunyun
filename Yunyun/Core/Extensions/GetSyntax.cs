using System;
using System.Linq;
using Discord.Commands;
using Yunyun.Core.Services;

namespace Yunyun.Core.Extensions
{
    public static partial class Extensions
    {
        public static string GetSyntax(this CommandInfo source)
            => $"```cs\n{ConfigurationService.Prefix}{string.Join(" | ", source.Aliases)} {string.Join(" ", source.Parameters.Select(p => p.IsOptional ? $"[{p.Name}]" : $"<{p.Name}>"))}```";
    }
}