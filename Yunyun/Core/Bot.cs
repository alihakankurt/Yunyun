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
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly LavaNode _lavaNode;

        public Bot()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.GuildVoiceStates,
                MessageCacheSize = 1000,
                AlwaysDownloadUsers = true
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = true,
                IgnoreExtraArgs = true
            });

            var collection = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(new HttpClient())
                .AddLavaNode(x => 
                {
                    x.SelfDeaf = true;
                    x.LogSeverity = LogSeverity.Verbose;
                });
            
            ProviderService.SetProvider(collection);

            _lavaNode = ProviderService.GetLavaNode();
            _commands.Log += DiscordLog;
            _client.Log += DiscordLog;
            _client.Ready += OnReady;
            _client.MessageReceived += OnMessage;
            _client.UserVoiceStateUpdated += OnVoiceStateUpdate;
        }

        public async Task RunAsync()
        {
            ConfigurationService.RunService();
            LavalinkService.RunService();

            if (string.IsNullOrWhiteSpace(ConfigurationService.Token))
                return;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), ProviderService.Provider);
            await _client.LoginAsync(TokenType.Bot, ConfigurationService.Token);
            await _client.StartAsync();
            
            await Task.Delay(-1);
        }

        private static Task DiscordLog(LogMessage arg)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] ({arg.Severity.ToString().ToUpper()}) {arg.Source} => {(arg.Exception is null ? arg.Message : arg.Exception.InnerException.Message)}");
            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            try
            {
                await _lavaNode.ConnectAsync();
            }
            catch (Exception exc)
            {
                throw exc.InnerException;
            }

            await _client.SetStatusAsync(UserStatus.Idle);
            await _client.SetGameAsync($"@{_client.CurrentUser.Username} play", null, ActivityType.Listening);
        }

        private async Task OnVoiceStateUpdate(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (!user.IsBot)
                if (before.VoiceChannel != null && after.VoiceChannel == null)
                    if (before.VoiceChannel.Users.Where(u => !u.IsBot).Count() < 1)
                        await LavalinkService.LeaveAsync(before.VoiceChannel);
        }

        private async Task OnMessage(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            
            if (message.Author.IsBot || message.Channel is IDMChannel)
                return;
            
            var argPos = 0;

            if (!(message.HasStringPrefix(ConfigurationService.Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;

            var result = await _commands.ExecuteAsync(context, argPos, ProviderService.Provider);

            if (!result.IsSuccess)
            {
                if (result.Error is CommandError.UnknownCommand)
                    return;

                else
                    await context.Channel.SendMessageAsync($"❗ {GetErrorMessage(result)}");
            }
        }

        private static string GetErrorMessage(IResult result)
        {
            return result.Error switch
            {
                CommandError.ParseFailed => "Malformed argument.",
                CommandError.BadArgCount => "Command did not have the right amount of parameters.",
                CommandError.ObjectNotFound => "Discord object was not found",
                CommandError.MultipleMatches => "Multiple commands were found. Please be more specific",
                CommandError.UnmetPrecondition => "A precondition for the command was not met.",
                CommandError.Exception => "An exception has occured during the command execution.",
                CommandError.Unsuccessful => "The command excecution was unsuccessfull.",
                _ => $"ERROR: {result}"
            };
        }
    }
}