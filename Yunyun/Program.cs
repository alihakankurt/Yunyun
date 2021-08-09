using System;
using Yunyun.Core;

namespace Yunyun
{
    class Program
    {
        static void Main(string[] args)
            => new Bot().RunAsync().GetAwaiter().GetResult();
    }
}
