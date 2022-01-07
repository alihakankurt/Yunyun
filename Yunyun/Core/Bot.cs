using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using Yunyun.Core.Services;

namespace Yunyun.Core
{
    public class Bot
    {
        private readonly DiscordSocketClient Client;
        private readonly CommandService Commands;
        private readonly LavaNode LavaNode;

        public Bot()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.GuildVoiceStates,
                MessageCacheSize = 200,
                AlwaysDownloadUsers = false
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = true,
                IgnoreExtraArgs = true
            });

            var collection = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(Commands)
                .AddSingleton(new HttpClient())
                .AddLavaNode(x => 
                {
                    x.SelfDeaf = true;
                    x.LogSeverity = LogSeverity.Verbose;
                });
            
            ProviderService.SetProvider(collection);

            LavaNode = ProviderService.GetLavaNode();
            Commands.Log += DiscordLog;
            Client.Log += DiscordLog;
            Client.Ready += OnReady;
            Client.MessageReceived += OnMessage;
        }

        public async Task RunAsync()
        {
            ConfigurationService.RunService();
            LavalinkService.RunService();

            if (string.IsNullOrWhiteSpace(ConfigurationService.Token))
                return;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), ProviderService.Provider);
            await Client.LoginAsync(TokenType.Bot, ConfigurationService.Token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task DiscordLog(LogMessage arg)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] ({arg.Severity.ToString().ToUpper()}) {arg.Source} => {((arg.Exception == null) ? arg.Message : arg.Exception.Message)}");
            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            try
            {
                await LavaNode.ConnectAsync();
            }
            catch
            {
                
            }

            await Client.SetStatusAsync(UserStatus.Idle);
            await Client.SetGameAsync($"@{Client.CurrentUser.Username} play • Version {ConfigurationService.Version}", null, ActivityType.Listening);
        }

        private async Task OnMessage(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(Client, message);
            
            if (message.Author.IsBot || message.Channel is IDMChannel)
                return;
            
            var argPos = 0;

            if (!(message.HasStringPrefix(ConfigurationService.Prefix, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos)))
                return;

            var result = await Commands.ExecuteAsync(context, argPos, ProviderService.Provider);

            if (!result.IsSuccess)
            {
                if (result.Error is CommandError.UnknownCommand)
                    return;


                var errorMessage = result.Error switch
                {
                    CommandError.ParseFailed => "Malformed argument.",
                    CommandError.BadArgCount => "Command did not have the right amount of parameters.",
                    CommandError.ObjectNotFound => "Discord object was not found",
                    CommandError.MultipleMatches => "Multiple commands were found. Please be more specific",
                    CommandError.UnmetPrecondition => "A precondition for the command was not met.",
                    CommandError.Exception => "An exception has occured during the command execution.",
                    CommandError.Unsuccessful => "The command excecution was unsuccessfull.",
                    _ => $"ERROR: {result.ErrorReason}"
                };
                await context.Channel.SendMessageAsync($"❗ {errorMessage}");
            }
        }
    }
}