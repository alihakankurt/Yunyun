using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Yunyun.Core.Services;

namespace Yunyun.Core;

public class Commands : ModuleBase<SocketCommandContext>
{
    [Name("Help")]
    [Command("help", RunMode = RunMode.Async)]
    [Summary("Shows the commands.")]
    public async Task HelpCommand([Remainder][Summary("Name of the command.")] string command = null)
        => await ReplyAsync(embed: LavalinkService.Help(command));

    [Name("Join")]
    [Command("join", RunMode = RunMode.Async)]
    [Summary("Joins to a voice channel.")]
    public async Task JoinCommand()
        => await ReplyAsync(await LavalinkService.JoinAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel, Context.Channel as ITextChannel));

    [Name("Leave")]
    [Command("leave", RunMode = RunMode.Async)]
    [Summary("Leaves from voice channel.")]
    public async Task LeaveCommand()
        => await ReplyAsync(await LavalinkService.LeaveAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel));

    [Name("Pause")]
    [Command("pause", RunMode = RunMode.Async)]
    [Summary("Pauses the playback.")]
    public async Task PauseCommand()
        => await ReplyAsync(await LavalinkService.PauseAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel));

    [Name("Resume")]
    [Command("resume", RunMode = RunMode.Async)]
    [Summary("Resumes the playback.")]
    public async Task ResumeCommand()
        => await ReplyAsync(await LavalinkService.ResumeAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel));

    [Name("Play")]
    [Command("play", RunMode = RunMode.Async)]
    [Alias("p")]
    [Summary("Plays a track in voice channel.")]
    public async Task PlayCommand([Remainder][Summary("The search query about track or URL.")] string query)
        => await ReplyAsync(await LavalinkService.PlayAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel, Context.Channel as ITextChannel, query.Trim('<', '>')));

    [Name("Stop")]
    [Command("stop", RunMode = RunMode.Async)]
    [Summary("Stops the playback.")]
    public async Task StopCommand()
        => await ReplyAsync(await LavalinkService.StopAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel));

    [Name("Volume")]
    [Command("volume", RunMode = RunMode.Async)]
    [Alias("vol")]
    [Summary("Sets the volume of playback.")]
    public async Task VolumeCommand([Remainder][Summary("An integer between 1 and 150.")] ushort volume)
            => await ReplyAsync(await LavalinkService.UpdateVolumeAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel, volume));

    [Name("Skip")]
    [Command("skip", RunMode = RunMode.Async)]
    [Alias("s")]
    [Summary("Skips the current track.")]
    public async Task SkipCommand()
            => await ReplyAsync(await LavalinkService.SkipAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel));

    [Name("Seek")]
    [Command("seek", RunMode = RunMode.Async)]
    [Alias("jump")]
    [Summary("Seeks the current track.")]
    public async Task SeekCommand([Remainder][Summary("The timestamp that you want to seek. (mm:ss)")] string timestamp)
        => await ReplyAsync(await LavalinkService.SeekAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel, timestamp));

    [Name("Equalizer")]
    [Command("equalizer", RunMode = RunMode.Async)]
    [Alias("eq")]
    [Summary("Sets the player's equalizer. (flat, boost, piano, metal)")]
    public async Task EqualizerCommand([Remainder][Summary("The name of the equalizer preset.")] string preset)
        => await ReplyAsync(await LavalinkService.EqualizerAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel, preset));

    [Name("Shuffle")]
    [Command("shuffle", RunMode = RunMode.Async)]
    [Summary("Shuffles the queue.")]
    public async Task ShuffleCommand()
            => await ReplyAsync(LavalinkService.Shuffle(Context.Guild, (Context.User as IVoiceState).VoiceChannel));

    [Name("Remove")]
    [Command("remove", RunMode = RunMode.Async)]
    [Alias("rm")]
    [Summary("Removes a track from queue.")]
    public async Task RemoveCommand([Summary("Track's position that want to remove.")] int position)
        => await ReplyAsync(LavalinkService.Remove(Context.Guild, (Context.User as IVoiceState).VoiceChannel, position - 1));

    [Name("Queue")]
    [Command("queue", RunMode = RunMode.Async)]
    [Alias("q")]
    [Summary("Shows the queue.")]
    public async Task QueueCommand([Remainder][Summary("The page that you want to see.")] ushort page = 1)
        => await ReplyAsync(embed: LavalinkService.GetQueueEmbed(Context.Guild, Context.User as IGuildUser, page));

    [Name("Now Playing")]
    [Command("nowplaying", RunMode = RunMode.Async)]
    [Alias("np")]
    [Summary("Shows the currently playing track.")]
    public async Task NowPlayingCommand()
        => await ReplyAsync(embed: await LavalinkService.GetNowPlayingEmbedAsync(Context.Guild));

    [Name("Lyrics")]
    [Command("lyrics", RunMode = RunMode.Async)]
    [Alias("l")]
    [Summary("Shows the currently playing track's lyrics.")]
    public async Task LyricsCommand([Remainder][Summary("The optional song name")] string song = null)
        => await ReplyAsync(embed: await LavalinkService.GetLyricsAsync(Context.Guild, song));

    [Name("Player")]
    [Command("player", RunMode = RunMode.Async)]
    [Summary("Shows the player.")]
    public async Task PlayerCommand()
        => await LavalinkService.SendPlayerEmbed(Context.Guild, Context.User, Context.Channel as ITextChannel);
}
