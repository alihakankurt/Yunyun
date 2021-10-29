using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Victoria;
using Yunyun.Core.Services;

namespace Yunyun.Core
{
    public static class Extensions
    {
        public static string GetSyntax(this CommandInfo source)
        {
            return $"```cs\n{ConfigurationService.Prefix}{string.Join(" | ", source.Aliases)} {string.Join(" ", source.Parameters.Select(p => p.IsOptional ? $"[{p.Name}]" : $"<{p.Name}>"))}```";
        }

        public static string GetTimeline(this LavaTrack track)
        {
            if (track.IsStream)
                return "`🔴 LIVE `";

            return $"**{track.Title} by {track.Author}**\n`{"───────────────────".Insert((int)(track.Position.TotalSeconds / track.Duration.TotalSeconds * 20), "⚪")} [{track.Position:hh\\:mm\\:ss} / {track.Duration:hh\\:mm\\:ss}]`";
        }
    }
}
