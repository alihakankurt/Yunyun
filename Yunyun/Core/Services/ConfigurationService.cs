using System;
using Microsoft.Extensions.Configuration;

namespace Yunyun.Core.Services
{
    public static class ConfigurationService
    {
        public static string Token { get; set; }
        public static string Prefix { get; set; }
        public static string Version { get; set; }

        public static void RunService()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                Token = config["Token"];
                Prefix = config["Prefix"];
                Version = config["Version"];
            }

            catch
            {
                Console.WriteLine("Configuration could not be loaded! Exiting...");
                Environment.Exit(0);
            }
        }
    }
}