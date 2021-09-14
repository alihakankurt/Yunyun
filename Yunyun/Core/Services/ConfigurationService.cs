using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Yunyun.Core.Services
{
    public static class ConfigurationService
    {
        public static string Token { get; set; }
        public static string Prefix { get; set; }

        private static string ConfigPath = "configuration.yaml";

        public static void RunService()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddYamlFile(ConfigPath)
                    .Build();
                Token = config["Token"];
                Prefix = config["Prefix"];
            }

            catch (Exception)
            {
                Console.WriteLine("Configuration could not be loaded! Exiting...");
                Environment.Exit(0);
            }
        }
    }
}