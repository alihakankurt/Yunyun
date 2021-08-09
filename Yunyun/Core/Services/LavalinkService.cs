using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Payloads;
using Victoria.Responses.Rest;

namespace Yunyun.Core.Services
{
    public static class LavalinkService
    {
        private static readonly LavaNode _lavaNode = ProviderService.GetLavaNode();
        private static readonly EqualizerBand[] Flat = new EqualizerBand[] { new EqualizerBand(0, 0.0), new EqualizerBand(1, 0), new EqualizerBand(2, 0.0), new EqualizerBand(3, 0.0), new EqualizerBand(4, 0.0), new EqualizerBand(5, 0.0), new EqualizerBand(6, 0.0), new EqualizerBand(7, 0.0), new EqualizerBand(8, 0.0), new EqualizerBand(9, 0.0), new EqualizerBand(10, 0.0), new EqualizerBand(11, 0.0), new EqualizerBand(12, 0.0), new EqualizerBand(13, 0.0), new EqualizerBand(14, 0.0) };
        private static readonly EqualizerBand[] Boost = new EqualizerBand[] { new EqualizerBand(0, -0.075), new EqualizerBand(1, 0.125), new EqualizerBand(2, 0.125), new EqualizerBand(3, 0.1), new EqualizerBand(4, 0.1), new EqualizerBand(5, 0.05), new EqualizerBand(6, 0.075), new EqualizerBand(7, 0.0), new EqualizerBand(8, 0.0), new EqualizerBand(9, 0.0), new EqualizerBand(10, 0.0), new EqualizerBand(11, 0.0), new EqualizerBand(12, 0.125), new EqualizerBand(13, 0.15), new EqualizerBand(14, 0.05) };
        private static readonly EqualizerBand[] Piano = new EqualizerBand[] { new EqualizerBand(0, -0.25), new EqualizerBand(1, -0.25), new EqualizerBand(2, -0.125), new EqualizerBand(3, 0.0), new EqualizerBand(4, 0.25), new EqualizerBand(5, 0.25), new EqualizerBand(6, 0.0), new EqualizerBand(7, -0.25), new EqualizerBand(8, -0.25), new EqualizerBand(9, 0.0), new EqualizerBand(10, 0.0), new EqualizerBand(11, 0.5), new EqualizerBand(12, 0.25), new EqualizerBand(13, -0.025), new EqualizerBand(14, 0.0) };
        private static readonly EqualizerBand[] Metal = new EqualizerBand[] { new EqualizerBand(0, 0.0), new EqualizerBand(1, 0.1), new EqualizerBand(2, 0.1), new EqualizerBand(3, 0.15), new EqualizerBand(4, 0.13), new EqualizerBand(5, 0.1), new EqualizerBand(6, 0.0), new EqualizerBand(7, 0.125), new EqualizerBand(8, 0.175), new EqualizerBand(9, 0.175), new EqualizerBand(10, 0.125), new EqualizerBand(11, 0.125), new EqualizerBand(12, 0.1), new EqualizerBand(13, 0.075), new EqualizerBand(14, 0.0) };

        public static void SetupLavalink()
        {
            _lavaNode.OnLog += OnLog;
            _lavaNode.OnTrackStarted += OnTrackStart;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnTrackStuck += OnTrackStuck;
            _lavaNode.OnTrackException += OnTrackException;
        }
        
        private static Task OnLog(LogMessage arg)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] ({arg.Severity.ToString().ToUpper()}) Lavalink => {(arg.Exception is null ? arg.Message : arg.Exception.InnerException.Message)}");
            return Task.CompletedTask;
        }

        private static async Task OnTrackStart(TrackStartEventArgs e)
        {
            var embed = new EmbedBuilder()
                .WithTitle("🎶Now Playing")
                .WithDescription($"{e.Track.Title} - {e.Track.Author}")
                .WithColor(255, 79, 0).Build();
            await e.Player.TextChannel.SendMessageAsync(embed: embed);
        }

        private static async Task OnTrackEnded(TrackEndedEventArgs e)
        {
            if (!e.Reason.ShouldPlayNext())
                return;

            if (e.Player.Queue.TryDequeue(out var queueable))
                return;

            if (!(queueable is LavaTrack track))
                return;

            await e.Player.PlayAsync(track);
        }

        private static async Task OnTrackStuck(TrackStuckEventArgs e)
        {
            if (e.Player.Queue.TryDequeue(out var queueable))
                return;

            if (!(queueable is LavaTrack track))
                return;

            await e.Player.PlayAsync(track);
        }

        private static async Task OnTrackException(TrackExceptionEventArgs e)
        {
            if (e.Player.Queue.TryDequeue(out var queueable))
                return;

            if (!(queueable is LavaTrack track))
                return;

            await e.Player.PlayAsync(track);
        }

        public static LavaPlayer GetPlayer(IGuild guild)
        {
            try
            {
                return _lavaNode.GetPlayer(guild);
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public static async Task JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel)
            => await _lavaNode.JoinAsync(voiceChannel, textChannel);
        
        public static async Task LeaveAsync(IVoiceChannel voiceChannel)
            => await _lavaNode.LeaveAsync(voiceChannel);

        public static async Task<SearchResponse> SearchAsync(string query)
            => await _lavaNode.SearchYouTubeAsync(query);

        public static async Task<SearchResponse> GetSearchResponseAsync(string query)
        {
            query = query.Trim('<', '>');
            return Uri.IsWellFormedUriString(query, UriKind.Absolute)
                ? await _lavaNode.SearchAsync(query)
                : await _lavaNode.SearchYouTubeAsync(query);
        }

        public static EqualizerBand[] GetEqualizer(string preset)
        {
            return preset.ToLower() switch
            {
                "flat" => Flat,
                "boost" => Boost,
                "piano" => Piano,
                "metal" => Metal,
                _ => null
            };
        }

        public static EqualizerBand[] GetEqualizer(int band, double gain)
        {
            if (band < 1 || band > 15)
                return null;

            if (gain < -10 || band > 10)
                return null;
            
            var equalizer = new EqualizerBand[] { new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0) };
            equalizer[band - 1] = new EqualizerBand(band - 1, gain / 10);
            return equalizer;
        }
    }
}