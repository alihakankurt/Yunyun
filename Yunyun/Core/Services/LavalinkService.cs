using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Victoria;
using Victoria.EventArgs;
using Victoria.Payloads;
using Victoria.Responses.Rest;

namespace Yunyun.Core.Services
{
    public static class LavalinkService
    {
        private static readonly HttpClient _client = ProviderService.GetService<HttpClient>();
        private static readonly LavaNode _lavaNode = ProviderService.GetLavaNode();
        private static readonly string _lyricsUrl = "https://api.genius.com/search?access_token={0}&q={1}&page=1&per_page=1";
        private static readonly EqualizerBand[] Flat = new EqualizerBand[] { new EqualizerBand(0, 0.0), new EqualizerBand(1, 0), new EqualizerBand(2, 0.0), new EqualizerBand(3, 0.0), new EqualizerBand(4, 0.0), new EqualizerBand(5, 0.0), new EqualizerBand(6, 0.0), new EqualizerBand(7, 0.0), new EqualizerBand(8, 0.0), new EqualizerBand(9, 0.0), new EqualizerBand(10, 0.0), new EqualizerBand(11, 0.0), new EqualizerBand(12, 0.0), new EqualizerBand(13, 0.0), new EqualizerBand(14, 0.0) };
        private static readonly EqualizerBand[] Boost = new EqualizerBand[] { new EqualizerBand(0, -0.075), new EqualizerBand(1, 0.125), new EqualizerBand(2, 0.125), new EqualizerBand(3, 0.1), new EqualizerBand(4, 0.1), new EqualizerBand(5, 0.05), new EqualizerBand(6, 0.075), new EqualizerBand(7, 0.0), new EqualizerBand(8, 0.0), new EqualizerBand(9, 0.0), new EqualizerBand(10, 0.0), new EqualizerBand(11, 0.0), new EqualizerBand(12, 0.125), new EqualizerBand(13, 0.15), new EqualizerBand(14, 0.05) };
        private static readonly EqualizerBand[] Piano = new EqualizerBand[] { new EqualizerBand(0, -0.25), new EqualizerBand(1, -0.25), new EqualizerBand(2, -0.125), new EqualizerBand(3, 0.0), new EqualizerBand(4, 0.25), new EqualizerBand(5, 0.25), new EqualizerBand(6, 0.0), new EqualizerBand(7, -0.25), new EqualizerBand(8, -0.25), new EqualizerBand(9, 0.0), new EqualizerBand(10, 0.0), new EqualizerBand(11, 0.5), new EqualizerBand(12, 0.25), new EqualizerBand(13, -0.025), new EqualizerBand(14, 0.0) };
        private static readonly EqualizerBand[] Metal = new EqualizerBand[] { new EqualizerBand(0, 0.0), new EqualizerBand(1, 0.1), new EqualizerBand(2, 0.1), new EqualizerBand(3, 0.15), new EqualizerBand(4, 0.13), new EqualizerBand(5, 0.1), new EqualizerBand(6, 0.0), new EqualizerBand(7, 0.125), new EqualizerBand(8, 0.175), new EqualizerBand(9, 0.175), new EqualizerBand(10, 0.125), new EqualizerBand(11, 0.125), new EqualizerBand(12, 0.1), new EqualizerBand(13, 0.075), new EqualizerBand(14, 0.0) };

        public static void RunService()
        {
            _lavaNode.OnLog += OnLog;
            _lavaNode.OnTrackStarted += OnTrackStarted;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnTrackStuck += OnTrackStuck;
            _lavaNode.OnTrackException += OnTrackException;
        }

        private static Task OnLog(LogMessage arg)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] ({arg.Severity.ToString().ToUpper()}) Lavalink => {(arg.Exception is null ? arg.Message : arg.Exception.InnerException.Message)}");
            return Task.CompletedTask;
        }

        private static async Task OnTrackStarted(TrackStartEventArgs e)
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

            if (!e.Player.Queue.TryDequeue(out var queueable))
                return;

            if (!(queueable is LavaTrack track))
                return;

            await e.Player.PlayAsync(track);
        }

        private static async Task OnTrackStuck(TrackStuckEventArgs e)
        {
            if (!e.Player.Queue.TryDequeue(out var queueable))
                return;

            if (!(queueable is LavaTrack track))
                return;

            await e.Player.PlayAsync(track);
        }

        private static async Task OnTrackException(TrackExceptionEventArgs e)
        {
            if (!e.Player.Queue.TryDequeue(out var queueable))
                return;

            if (!(queueable is LavaTrack track))
                return;

            await e.Player.PlayAsync(track);
        }

        public static LavaPlayer GetPlayer(IGuild guild)
            => _lavaNode.TryGetPlayer(guild, out var player) ? player : null;

        public static async Task JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel)
            => await _lavaNode.JoinAsync(voiceChannel, textChannel);

        public static async Task MoveAsync(SocketVoiceChannel channel)
            => await _lavaNode.MoveChannelAsync(channel);

        public static async Task LeaveAsync(IVoiceChannel voiceChannel)
            => await _lavaNode.LeaveAsync(voiceChannel);

        public static async Task<SearchResponse> SearchAsync(string query)
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

            if (gain < -2.5 || band > 10)
                return null;

            var equalizer = new EqualizerBand[] { new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0), new EqualizerBand(0, 0.0) };
            equalizer[band - 1] = new EqualizerBand(band - 1, gain / 10);
            return equalizer;
        }

        public async static Task<GeniusResponse> SearchGeniusAsync(string query)
        {
            try
            {
                query = query.Split('(')[0].Split('[')[0];
                var json = JObject.Parse(await _client.GetStringAsync(string.Format(_lyricsUrl, ConfigurationService.GeniusToken, query)));
                var hits = json["response"]["hits"][0]["result"];
                var title = (string)hits["full_title"];
                var lyricsUrl = $"https://genius.com{hits["path"]}";
                var thumbnail = (string)hits["header_image_url"];
                
                var html = (await _client.GetStringAsync(lyricsUrl))[120000..];
                int index = html.IndexOf("<div class=\"lyrics\"");
                int length;
                string lyrics;
                
                if (index > -1)
                {
                    index += html[index..].IndexOf("<p>") + 3;
                    length = html[index..].IndexOf("</p>");
                    lyrics = html[index..(index + length)].Replace("<br>", Environment.NewLine);
                }
                
                else
                {
                    index = html.IndexOf("<div data-lyrics-container=\"true\"");
                    index += html[index..].IndexOf(">") + 1;
                    length = html[index..].IndexOf("</div>");
                    lyrics = HttpUtility.HtmlDecode(html[index..(index + length)].Replace("<br/>", Environment.NewLine));
                }
                index = 0;
                for (int i = 0; i < lyrics.Length; i++)
                {
                    if (lyrics[i] == '<')
                    {
                        index = i;
                    }

                    else if (lyrics[i] == '>')
                    {
                        lyrics = lyrics.Remove(index, i - index + 1);
                        i = index;
                    }
                }
                lyrics = string.Join("\n", lyrics.Split("\n", StringSplitOptions.RemoveEmptyEntries));
                return new GeniusResponse(title, lyrics, lyricsUrl, thumbnail);
            }

            catch
            {
                return null;
            }
        }

        public class GeniusResponse
        {
            public string Title { get; set; }
            public string Lyrics { get; set; }
            public string Url { get; set; }
            public string Thumbnail { get; set; }

            public GeniusResponse(string title, string lyrics, string url, string thumbnail)
            {
                Title = title;
                Lyrics = lyrics;
                Url = url;
                Thumbnail = thumbnail;
            }
        }
    }
}