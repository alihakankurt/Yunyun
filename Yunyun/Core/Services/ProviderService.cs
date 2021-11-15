using System;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace Yunyun.Core.Services
{
    public static class ProviderService
    {
        public static IServiceProvider Provider;

        public static void SetProvider(IServiceCollection collection)
            => Provider = collection.BuildServiceProvider();

        public static T GetService<T>()
            => Provider.GetRequiredService<T>();

        public static LavaNode GetLavaNode()
            => Provider.GetRequiredService<LavaNode>();
    }
}