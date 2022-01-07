using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Filters;
using Victoria.Responses.Search;

namespace Yunyun.Core.Services;

public static class LavalinkService
{
    private static readonly DiscordSocketClient Client = ProviderService.GetService<DiscordSocketClient>();
    private static readonly CommandService Commands = ProviderService.GetService<CommandService>();
    private static readonly LavaNode LavaNode = ProviderService.GetLavaNode();
    private static readonly Dictionary<ulong, ulong> PlayerEmbeds = new();
    private static readonly List<Emoji> Emotes = new() { new Emoji("❌"), new Emoji("⏹"), new Emoji("⏸"), new Emoji("▶"), new Emoji("🔼"), new Emoji("🔽"), new Emoji("⏭"), new Emoji("🔀") };
    private static readonly EqualizerBand[] Flat = new EqualizerBand[] { new EqualizerBand(0, 0.0), new EqualizerBand(1, 0), new EqualizerBand(2, 0.0), new EqualizerBand(3, 0.0), new EqualizerBand(4, 0.0), new EqualizerBand(5, 0.0), new EqualizerBand(6, 0.0), new EqualizerBand(7, 0.0), new EqualizerBand(8, 0.0), new EqualizerBand(9, 0.0), new EqualizerBand(10, 0.0), new EqualizerBand(11, 0.0), new EqualizerBand(12, 0.0), new EqualizerBand(13, 0.0), new EqualizerBand(14, 0.0) };
    private static readonly EqualizerBand[] Boost = new EqualizerBand[] { new EqualizerBand(0, -0.075), new EqualizerBand(1, 0.125), new EqualizerBand(2, 0.125), new EqualizerBand(3, 0.1), new EqualizerBand(4, 0.1), new EqualizerBand(5, 0.05), new EqualizerBand(6, 0.075), new EqualizerBand(7, 0.0), new EqualizerBand(8, 0.0), new EqualizerBand(9, 0.0), new EqualizerBand(10, 0.0), new EqualizerBand(11, 0.0), new EqualizerBand(12, 0.125), new EqualizerBand(13, 0.15), new EqualizerBand(14, 0.05) };
    private static readonly EqualizerBand[] Piano = new EqualizerBand[] { new EqualizerBand(0, -0.25), new EqualizerBand(1, -0.25), new EqualizerBand(2, -0.125), new EqualizerBand(3, 0.0), new EqualizerBand(4, 0.25), new EqualizerBand(5, 0.25), new EqualizerBand(6, 0.0), new EqualizerBand(7, -0.25), new EqualizerBand(8, -0.25), new EqualizerBand(9, 0.0), new EqualizerBand(10, 0.0), new EqualizerBand(11, 0.5), new EqualizerBand(12, 0.25), new EqualizerBand(13, -0.025), new EqualizerBand(14, 0.0) };
    private static readonly EqualizerBand[] Metal = new EqualizerBand[] { new EqualizerBand(0, 0.0), new EqualizerBand(1, 0.1), new EqualizerBand(2, 0.1), new EqualizerBand(3, 0.15), new EqualizerBand(4, 0.13), new EqualizerBand(5, 0.1), new EqualizerBand(6, 0.0), new EqualizerBand(7, 0.125), new EqualizerBand(8, 0.175), new EqualizerBand(9, 0.175), new EqualizerBand(10, 0.125), new EqualizerBand(11, 0.125), new EqualizerBand(12, 0.1), new EqualizerBand(13, 0.075), new EqualizerBand(14, 0.0) };

    public static void RunService()
    {
        Client.UserVoiceStateUpdated += UserVoiceStateUpdated;
        Client.ButtonExecuted += ButtonExecuted;

        LavaNode.OnLog += OnLog;
        LavaNode.OnTrackStarted += OnTrackStarted;
        LavaNode.OnTrackEnded += OnTrackEnded;
        LavaNode.OnTrackStuck += OnTrackStuck;
        LavaNode.OnTrackException += OnTrackException;
    }

    private static async Task ButtonExecuted(SocketMessageComponent component)
    {
        var guild = Client.GetGuild(ulong.Parse(component.Data.CustomId[..18]));
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return;

        if (!PlayerEmbeds.ContainsKey(guild.Id))
            PlayerEmbeds[guild.Id] = component.Message.Id;

        else if (PlayerEmbeds[guild.Id] != component.Message.Id)
        {
            await component.Message.DeleteAsync();
            return;
        }

        var voiceChannel = (component.User as IVoiceState).VoiceChannel;
        switch (component.Data.CustomId[19..])
        {
            case "leave":
                await LeaveAsync(guild, voiceChannel);
                await component.Message.DeleteAsync();
                return;

            case "stop":
                await StopAsync(guild, voiceChannel);
                break;

            case "pause":
                await PauseAsync(guild, voiceChannel);
                break;

            case "resume":
                await ResumeAsync(guild, voiceChannel);
                break;

            case "volumeUp":
                await UpdateVolumeAsync(guild, voiceChannel, (ushort)((player.Volume > 145) ? 150 : player.Volume + 5));
                break;

            case "volumeDown":
                await UpdateVolumeAsync(guild, voiceChannel, (ushort)((player.Volume < 5) ? 0 : player.Volume - 5));
                break;

            case "skip":
                await SkipAsync(guild, voiceChannel);
                break;

            case "shuffle":
                Shuffle(guild, voiceChannel);
                break;

            default:
                return;
        }
        await component.Message.ModifyAsync(x => x.Embed = GetPlayerEmbed(player, component.User));
        await component.DeferAsync();
    }

    private static async Task UserVoiceStateUpdated(SocketUser arg, SocketVoiceState before, SocketVoiceState after)
    {
        if (arg.IsBot)
            return;

        if (arg is not IGuildUser guildUser)
            return;

        if (!LavaNode.TryGetPlayer(guildUser.Guild, out LavaPlayer player))
            return;

        if (before.VoiceChannel != null && after.VoiceChannel == null)
            foreach (var user in await player.VoiceChannel.GetUsersAsync().FlattenAsync())
                if (!user.IsBot)
                    return;

        await LavaNode.LeaveAsync(player.VoiceChannel);
    }

    private static Task OnLog(LogMessage arg)
    {
        Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] ({arg.Severity.ToString().ToUpper()}) Lavalink => {(arg.Exception is null ? arg.Message : arg.Exception.InnerException.Message)}");
        return Task.CompletedTask;
    }

    private static async Task OnTrackStarted(TrackStartEventArgs e)
    {
        var embed = new EmbedBuilder()
            .WithTitle("🎶 Now Playing")
            .WithDescription($"{e.Track.Title} - {e.Track.Author}")
            .WithColor(255, 79, 0)
            .Build();

        await e.Player.TextChannel.SendMessageAsync(embed: embed);
    }

    private static async Task OnTrackEnded(TrackEndedEventArgs e)
    {
        if (e.Reason == TrackEndReason.Stopped)
            return;

        if (!e.Player.Queue.TryDequeue(out var queueable))
            return;

        if (queueable is not LavaTrack track)
            return;

        await e.Player.PlayAsync(track);
    }

    private static async Task OnTrackStuck(TrackStuckEventArgs e)
    {
        if (!e.Player.Queue.TryDequeue(out var queueable))
            return;

        if (queueable is not LavaTrack track)
            return;

        await e.Player.PlayAsync(track);
    }

    private static async Task OnTrackException(TrackExceptionEventArgs e)
    {
        if (!e.Player.Queue.TryDequeue(out var queueable))
            return;

        if (queueable is not LavaTrack track)
            return;

        await e.Player.PlayAsync(track);
    }

    public static Embed Help(string commandName)
    {
        var command = Commands.Commands.FirstOrDefault(x => x.Aliases.Contains(commandName));
        return ((command == null)
            ? new EmbedBuilder()
                .WithTitle($"{Client.CurrentUser.Username}'s Commands")
                .WithDescription(string.Join("\n", Commands.Commands.Select(c => $"`{ConfigurationService.Prefix}{string.Join("|", c.Aliases)}` -> {c.Summary}")))
            : new EmbedBuilder()
                .WithTitle($"{command.Name}")
                .AddField("Syntax", $"```cs\n{ConfigurationService.Prefix}{string.Join(" | ", command.Aliases)} {string.Join(" ", command.Parameters.Select(p => p.IsOptional ? $"[{p.Name}]" : $"<{p.Name}>"))}```", false)
                .AddField("Summary", command.Summary, false)
                .AddField("Parameters", (command.Parameters.Count() > 0) ? string.Join("\n", command.Parameters.Select(p => $"{(p.IsOptional ? $"`[{p}]`" : $"`<{p}>`")} -> {p.Summary}")) : "None", false)
                ).WithColor(255, 79, 0)
                .WithCurrentTimestamp()
                .Build();
    }

    public static async Task<string> JoinAsync(IGuild guild, IVoiceChannel voiceChannel, ITextChannel textChannel)
    {
        if (LavaNode.HasPlayer(guild))
            return $"I'm already connected to a voice channel!";

        if (voiceChannel == null)
            return "No voice channel is provided!";

        var player = await LavaNode.JoinAsync(voiceChannel, textChannel);
        await player.UpdateVolumeAsync(75);
        Console.WriteLine(player.Volume);
        return $"Joined to {voiceChannel.Mention} and bound to {textChannel.Mention}.";
    }

    public static async Task<string> LeaveAsync(IGuild guild, IVoiceChannel voiceChannel)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return $"I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        string leaveMessage = $"Leaved from {player.VoiceChannel.Mention}";
        await LavaNode.LeaveAsync(player.VoiceChannel);
        return leaveMessage;
    }

    public static async Task<string> PauseAsync(IGuild guild, IVoiceChannel voiceChannel)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return "I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        if (player.PlayerState != PlayerState.Playing)
            return "Playback is not playing!";

        await player.PauseAsync();
        return "Playback paused.";
    }

    public static async Task<string> ResumeAsync(IGuild guild, IVoiceChannel voiceChannel)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return "I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        if (player.PlayerState != PlayerState.Paused)
            return "Playback is not paused!";

        await player.ResumeAsync();
        return "Playback resumed.";
    }

    public static async Task<string> PlayAsync(IGuild guild, IVoiceChannel voiceChannel, ITextChannel textChannel, string query)
    {
        string text = string.Empty;
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
        {
            if (voiceChannel == null)
                return "No voice channel is provided!";

            player = await LavaNode.JoinAsync(voiceChannel, textChannel);
            await player.UpdateVolumeAsync(75);
            text = $"Joined to {voiceChannel.Mention} and bound to {textChannel.Mention}.";
        }

        else if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        var response = await LavaNode.SearchYouTubeAsync(query);
        if (response.Status == SearchStatus.LoadFailed || response.Status == SearchStatus.NoMatches)
            return $"{text}\nNo tracks could be found with given query!";

        if (response.Status == SearchStatus.PlaylistLoaded)
            player.Queue.Enqueue(response.Tracks);

        else
            player.Queue.Enqueue(response.Tracks.First());

        if (player.PlayerState == PlayerState.None || player.PlayerState == PlayerState.Stopped)
            if (player.Queue.TryDequeue(out LavaTrack track))
                await player.PlayAsync(track);

        return (response.Status == SearchStatus.PlaylistLoaded) ? $"{text}\nThe playlist `{response.Playlist.Name}` loaded with `{response.Tracks.Count}` tracks." : $"{text}\n`{response.Tracks.First().Title}` added to queue.";
    }

    public static async Task<string> StopAsync(IGuild guild, IVoiceChannel voiceChannel)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return "I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        await player.StopAsync();
        return "Playback stopped.";
    }

    public static async Task<string> UpdateVolumeAsync(IGuild guild, IVoiceChannel voiceChannel, ushort volume)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return "I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        if (volume > 150 || volume < 1)
            return "Volume must be between 1 and 150!";

        await player.UpdateVolumeAsync(volume);
        return $"Volume updated to `{volume}`.";
    }

    public static async Task<string> SkipAsync(IGuild guild, IVoiceChannel voiceChannel)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return "I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        await ((player.Queue.Count == 0) ? player.SeekAsync(player.Track.Duration) : player.SkipAsync());
        return "Skipped.";
    }

    public static async Task<string> SeekAsync(IGuild guild, IVoiceChannel voiceChannel, string timestamp)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return "I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        if (player.PlayerState != PlayerState.Playing)
            return "Nothing is playing right now!";

        var match = Regex.Match(timestamp, "^([0-9]{1,2})[:.]?([0-9]{1,2})?$");
        if (!match.Success)
            return "Invalid timestamp!";

        var result = TimeSpan.FromSeconds(match.Groups.Values.ElementAt(2).Success
            ? Convert.ToInt32(match.Groups.Values.ElementAt(1).Value) * 60 + Convert.ToInt32(match.Groups.Values.ElementAt(2).Value)
            : Convert.ToInt32(match.Groups.Values.ElementAt(1).Value));

        if (player.Track.Duration.CompareTo(result) < 1)
            return "Timestamp can't exceed the current track's duration!";

        await player.SeekAsync(result);
        return $"Seeked to {result:hh\\:mm\\:ss}";
    }

    public static async Task<string> EqualizerAsync(IGuild guild, IVoiceChannel voiceChannel, string preset)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return "I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        var equalizer = preset.ToLower() switch
        {
            "flat" => Flat,
            "boost" => Boost,
            "piano" => Piano,
            "metal" => Metal,
            _ => null
        };
        if (equalizer == null)
            return "Invalid equalizer preset!";

        await player.EqualizerAsync(equalizer);
        return $"Equalizer adjusted to `{nameof(equalizer).ToLower()}`.";
    }

    public static string Shuffle(IGuild guild, IVoiceChannel voiceChannel)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return "I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        player.Queue.Shuffle();
        return "Queue shuffled.";
    }

    public static string Remove(IGuild guild, IVoiceChannel voiceChannel, int index)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return "I'm not connected to a voice channel!";

        if (voiceChannel != player.VoiceChannel)
            return $"You need to be in {voiceChannel.Mention} for do that!";

        if (player.Queue.Count <= index || index < 0)
            return "Invalid track position!";

        var track = player.Queue.RemoveAt(index);
        return $"`{track.Title}` removed from the queue.";
    }

    public static Embed GetQueueEmbed(IGuild guild, IGuildUser user, int page)
    {
        page *= 10;
        var builder = new EmbedBuilder()
            .WithColor(255, 79, 0)
            .WithFooter(footer =>
            {
                footer.Text = $"Requested by {user}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .WithCurrentTimestamp();

        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return builder
                .WithDescription("-- Empty --")
                .Build();

        return builder
            .WithDescription(string.Join("\n", player.Queue.Where((t, i) => i >= page - 10 && i < page).Select((t, i) => $"`{i + 1})` {t.Title} - {t.Author}")))
            .Build();
    }

    public static async Task<Embed> GetNowPlayingEmbedAsync(IGuild guild)
    {
        var builder = new EmbedBuilder()
            .WithTitle("🎶Now Playing")
            .WithColor(255, 79, 0)
            .WithCurrentTimestamp();

        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return builder
                .WithDescription("Not connected!")
                .Build();

        if (player.PlayerState != PlayerState.Playing)
            return builder
                .WithDescription("Nothing!")
                .Build();

        return builder
            .WithDescription(player.Track.IsStream ? "`🔴 LIVE `" : $"{player.Track.Title}\n`{"───────────────────".Insert((int)((player.Track.Position.TotalSeconds / player.Track.Duration.TotalSeconds) * 20), "⚪")} [{player.Track.Position:hh\\:mm\\:ss} / {player.Track.Duration:hh\\:mm\\:ss}]`")
            .WithThumbnailUrl(await player.Track.FetchArtworkAsync())
            .Build();
    }

    public static async Task<Embed> GetLyricsAsync(IGuild guild, string query)
    {
        var builder = new EmbedBuilder()
            .WithColor(255, 79, 0)
            .WithCurrentTimestamp();

        if (LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            query ??= (player.PlayerState == PlayerState.Playing) ? player.Track.Title : null;

        if (query == null)
            builder
                .WithDescription("Not connected!")
                .Build();

        var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
        var thumbnail = await player.Track.FetchArtworkAsync();
        if (lyrics == null)
            return builder
                .WithDescription("No tracks could be found!")
                .Build();

        return builder
            .WithDescription((lyrics.Length > 2048) ? $"{lyrics[..2045]}..." : lyrics)
            .WithThumbnailUrl(thumbnail)
            .Build();
    }

    public static async Task SendPlayerEmbed(IGuild guild, IUser user, ITextChannel textChannel)
    {
        if (!LavaNode.TryGetPlayer(guild, out LavaPlayer player))
            return;

        if (PlayerEmbeds.ContainsKey(guild.Id))
            await player.TextChannel.DeleteMessageAsync(PlayerEmbeds[guild.Id]);

        var embed = GetPlayerEmbed(player, user);
        var component = new ComponentBuilder()
            .WithButton("Leave", $"{guild.Id}-leave", ButtonStyle.Secondary, Emotes[0], null, false, 0)
            .WithButton("Stop", $"{guild.Id}-stop", ButtonStyle.Secondary, Emotes[1], null, false, 0)
            .WithButton("Pause", $"{guild.Id}-pause", ButtonStyle.Secondary, Emotes[2], null, false, 0)
            .WithButton("Resume", $"{guild.Id}-resume", ButtonStyle.Secondary, Emotes[3], null, false, 0)
            .WithButton("Volume + 5", $"{guild.Id}-volumeUp", ButtonStyle.Secondary, Emotes[4], null, false, 1)
            .WithButton("Volume - 5", $"{guild.Id}-volumeDown", ButtonStyle.Secondary, Emotes[5], null, false, 1)
            .WithButton("Skip", $"{guild.Id}-skip", ButtonStyle.Secondary, Emotes[6], null, false, 1)
            .WithButton("Shuffle", $"{guild.Id}-shuffle", ButtonStyle.Secondary, Emotes[7], null, false, 1)
            .Build();

        var message = await player.TextChannel.SendMessageAsync(embed: embed, components: component);
        PlayerEmbeds[guild.Id] = message.Id;
    }

    public static Embed GetPlayerEmbed(LavaPlayer player, IUser user)
    {
        return new EmbedBuilder()
            .WithFooter(footer =>
            {
                footer.Text = $"Invoked by {user.Username}";
                footer.IconUrl = user.GetAvatarUrl();
            })
            .WithTitle($"{Client.CurrentUser.Username} Player")
            .AddField("Playing", player.Track == null ? "Nothing" : player.Track.Title, true)
            .AddField("State", player.PlayerState, true)
            .AddField("Volume", player.Volume, true)
            .AddField("Voice Channel", player.VoiceChannel.Mention, true)
            .AddField("Text Channel", player.TextChannel.Mention, true)
            .WithColor(255, 79, 0)
            .WithCurrentTimestamp()
            .Build();
    }
}
