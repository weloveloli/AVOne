// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.DL
{
    using System.Text;
    using AVOne.Providers.Official.Download.Parser;
    using AVOne.Providers.Official.Download.Parser.DashParser;

    public class DashDL : BaseDL
    {
        protected readonly HttpClient _httpClient;

        /// <summary>
        /// Init DashDL.
        /// </summary>
        /// <param name="timeout">Set http request timeout.(millisecond)</param>
        /// <param name="proxy">Set http or socks5 proxy.
        /// http://{hostname}:{port} or socks5://{hostname}:{port}</param>
        public DashDL(int timeout = 60000, string? proxy = null)
            : this(CreateHttpClient(timeout, proxy))
        {
        }

        /// <summary>
        /// Init DashDL.
        /// </summary>
        /// <param name="httpClient">Set http client.</param>
        public DashDL(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get mpd manifest by url.
        /// </summary>
        /// <param name="url">Set mpd download url.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        public async Task<(string data, string url)> GetManifestAsync(
            string url, string header = "", CancellationToken token = default)
        {
            return await GetStringAsync(_httpClient, url, header, token);
        }

        /// <summary>
        /// Get mpd by url.
        /// </summary>
        /// <param name="url">Set mpd download url.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        public async Task<Mpd> GetMpdAsync(
            string url, string header = "", CancellationToken token = default)
        {
            var manifest = await GetManifestAsync(url, header, token);
            var parser = new MpdParser();
            return parser.Parse(manifest.data, manifest.url);
        }

        /// <summary>
        /// Parse mpd by manifest.
        /// </summary>
        /// <param name="manifest">Set mpd manifest.</param>
        /// <param name="url">Set mpd url.</param>
        /// <returns></returns>
        public Mpd ParseMpd(string manifest, string url = "")
        {
            var parser = new MpdParser();
            return parser.Parse(manifest, url);
        }

        /// <summary>
        /// Expand segmentBase to segmentList.
        /// </summary>
        /// <param name="representation">Set representation.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        public async Task ExpandSegmentBase(Representation representation,
            string header = "", CancellationToken token = default)
        {
            var segmentList = representation.SegmentList;
            var segmentBase = representation.SegmentBase;
            if (segmentBase != null)
            {
                if (string.IsNullOrEmpty(segmentBase.BaseUrl))
                {
                    throw new Exception("Not found segmentBase baseUrl.");
                }

                var initialization = segmentBase.Initialization;
                var rangeFrom = initialization?.Range?.From;
                var rangeTo = initialization?.Range?.To;

                if (rangeFrom == null || rangeTo == null)
                {
                    throw new Exception("Not found segmentBase initialization range.");
                }

                var (respHeaders, _) = await GetHeadersAsync(_httpClient,
                    segmentBase.BaseUrl, header, 0, 0, token);

                var contentLength = respHeaders?.ContentRange?.Length;
                if (contentLength == null)
                {
                    throw new Exception("Not found segmentBase content-length.");
                }

                segmentList.Initialization = new Initialization
                {
                    SourceURL = segmentBase.BaseUrl,
                    Range = new IndexRange
                    {
                        From = rangeFrom.Value,
                        To = rangeTo.Value,
                    }
                };

                var chunkSize = 4 * 1024 * 1024;

                List<(long from, long to)> calcChunks()
                {
                    var ranges = new List<(long from, long to)>();

                    var current = rangeTo.Value + 1;
                    var length = contentLength.Value - current;

                    while (true)
                    {
                        var size = Math.Min(length, chunkSize);
                        if (size == 0)
                        {
                            break;
                        }

                        ranges.Add((current, current + size - 1));
                        current += size;
                        length -= size;
                    }
                    return ranges;
                }
                var chunks = calcChunks();

                foreach (var (from, to) in chunks)
                {
                    segmentList.SegmentUrls.Add(new SegmentUrl
                    {
                        Media = segmentBase.BaseUrl,
                        MediaRange = new IndexRange
                        {
                            From = from,
                            To = to,
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Convert representation to m3u8 manifest.
        /// </summary>
        /// <param name="representation">Set mpd representation.</param>
        /// <returns></returns>
        public string ToM3U8(Representation representation)
        {
            var m3u8 = new StringBuilder();
            var segmentList = representation.SegmentList;

            _ = m3u8.AppendLine("#EXTM3U");
            _ = m3u8.AppendLine("#EXT-X-VERSION:3");
            _ = m3u8.AppendLine("#EXT-X-PLAYLIST-TYPE:VOD");

            if (segmentList.Initialization != null)
            {
                var initialization = segmentList.Initialization;
                _ = m3u8.Append($@"#EXT-X-MAP:URI=""{initialization.SourceURL}""");
                if (initialization.Range != null)
                {
                    var from = initialization.Range.From;
                    var to = initialization.Range.To;
                    _ = m3u8.Append($@",BYTERANGE=""{to - from + 1}@{from}""");
                }
                _ = m3u8.AppendLine();
            }

            foreach (var segmentUrl in segmentList.SegmentUrls)
            {
                var duration = (double)segmentUrl.Duration / segmentUrl.Timescale;
                _ = m3u8.AppendLine($"#EXTINF:{duration.ToString("0.00")}");
                if (segmentUrl.MediaRange != null)
                {
                    var from = segmentUrl.MediaRange.From;
                    var to = segmentUrl.MediaRange.To;
                    _ = m3u8.AppendLine($"#EXT-X-BYTERANGE:{to - from + 1}@{from}");
                }
                _ = m3u8.AppendLine(segmentUrl.Media);
            }

            _ = m3u8.AppendLine("#EXT-X-ENDLIST");
            return m3u8.ToString();
        }

        /// <summary>
        /// Convert representation to media playlist.
        /// </summary>
        /// <param name="representation">Set mpd representation.</param>
        /// <param name="url">Set mpd url.</param>
        /// <returns></returns>
        public MediaPlaylist ToMediaPlaylist(Representation representation, string url = "")
        {
            var parser = new MediaPlaylistParser();
            var manifest = ToM3U8(representation);
            return parser.Parse(manifest, url);
        }
    }
}
