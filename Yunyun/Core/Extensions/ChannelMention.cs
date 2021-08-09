using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Yunyun.Core.Extensions
{
    public static partial class Extensions
    {
        public static string Mention(this IChannel source)
            => $"<#{source.Id}>";
    }
}
