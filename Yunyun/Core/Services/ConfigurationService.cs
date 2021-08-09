using System;
using Microsoft.Extensions.Configuration;

namespace Yunyun.Core.Services
{
    public static class ConfigurationService
    {
        public static string Token { get; private set; }
        public static string Prefix { get; private set; }

        public static void ReadConfigFile()
        {   
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddYamlFile("configuration.yaml")
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