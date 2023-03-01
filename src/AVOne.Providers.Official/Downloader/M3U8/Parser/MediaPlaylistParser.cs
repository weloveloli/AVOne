// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8.Parser
{
    using System.Text.RegularExpressions;
    using AVOne.Providers.Official.Downloader.M3U8.Extensions;

    internal class MediaPlaylistParser : BaseParser
    {
        // Regex refer to:
        // https://github.com/videojs/m3u8-parser
        public MediaPlaylist Parse(string manifest, string m3u8Url = "")
        {
            using var reader = new StringReader(manifest);
            var playlist = new MediaPlaylist();
            playlist.Url = m3u8Url;
            playlist.Manifest = manifest;

            var segment = new Segment();
            var key = new SegmentKey();
            var byteRange = null as ByteRange;
            var segmentMap = null as SegmentMap;
            var part = new Part();
            playlist.Parts.Add(part);

            var index = 0L;
            var partIndex = 0;

            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                if (!line.StartsWith("#"))
                {
                    segment.Uri = string.IsNullOrEmpty(m3u8Url) ?
                        line : m3u8Url.CombineUri(line);

                    // Set byte range
                    if (byteRange != null)
                    {
                        segment.ByteRange = byteRange;
                        byteRange = null;
                    }

                    // Set key
                    segment.Key = new SegmentKey
                    {
                        Method = key.Method,
                        Uri = key.Uri,
                        IV = key.IV
                    };
                    if (key.Method != "NONE" && key.IV == "")
                    {
                        var index16 = Convert.ToString(segment.Index, 16);
                        segment.Key.IV = $"0x{index16.PadLeft(32, '0')}";
                    }

                    part.Segments.Add(segment);
                    segment = new Segment();
                    continue;
                }

                var match = Regex.Match(line, @"^#EXTM3U");
                if (match.Success)
                {
                    playlist.IsM3U = true;
                    continue;
                }
                match = Regex.Match(line, @"^#EXTINF:?([0-9\.]*)?,?(.*)?$");
                if (match.Success)
                {
                    var duration = match.Groups[1].Value;
                    if (duration != "")
                    {
                        segment.Duration = double.Parse(duration);
                        if (segment.Duration == 0)
                            segment.Duration = 0.01;
                        playlist.TotalDuration += segment.Duration;
                    }
                    var title = match.Groups[2].Value;
                    if (title != "")
                        segment.Title = title;
                    segment.Index = index;
                    index++;
                    continue;
                }
                match = Regex.Match(line, "^#EXT-X-TARGETDURATION:?([0-9.]*)?");
                if (match.Success)
                {
                    var targetDuration = match.Groups[1].Value;
                    if (targetDuration != "")
                        playlist.TargetDuration = (int)double.Parse(targetDuration);
                    continue;
                }
                match = Regex.Match(line, "^#EXT-X-VERSION:?([0-9.]*)?");
                if (match.Success)
                {
                    var version = match.Groups[1].Value;
                    if (version != "")
                        playlist.Version = int.Parse(version);
                    continue;
                }
                match = Regex.Match(line, @"^#EXT-X-MEDIA-SEQUENCE:?(\-?[0-9.]*)?");
                if (match.Success)
                {
                    var mediaSequence = match.Groups[1].Value;
                    if (mediaSequence != "")
                    {
                        playlist.MediaSequence = long.Parse(mediaSequence);
                        index = playlist.MediaSequence;
                    }
                    continue;
                }
                match = Regex.Match(line, @"^#EXT-X-PLAYLIST-TYPE:?(.*)?$");
                if (match.Success)
                {
                    var playlistType = match.Groups[1].Value;
                    if (playlistType != "")
                        playlist.PlaylistType = playlistType;
                    continue;
                }
                match = Regex.Match(line, @"^#EXT-X-ENDLIST");
                if (match.Success)
                {
                    playlist.EndList = true;
                    continue;
                }
                match = Regex.Match(line, @"^#EXT-X-DISCONTINUITY");
                if (match.Success)
                {
                    segment.Discontinuity = true;
                    part = new Part();
                    playlist.Parts.Add(part);
                    partIndex++;
                    part.PartIndex = partIndex;
                    // segmentMap = null;
                    continue;
                }
                match = Regex.Match(line, @"^#EXT-X-DISCONTINUITY-SEQUENCE:?(\-?[0-9.]*)?");
                if (match.Success)
                {
                    var discontinuitySequence = match.Groups[1].Value;
                    if (discontinuitySequence != "")
                    {
                        playlist.DiscontinuitySequence = int.Parse(discontinuitySequence);
                        partIndex = playlist.DiscontinuitySequence;
                    }
                    continue;
                }
                match = Regex.Match(line, @"^#EXT-X-KEY:?(.*)$");
                if (match.Success)
                {
                    key = new SegmentKey();
                    var attributes = match.Groups[1].Value;
                    if (attributes != "")
                    {
                        var attrs = ParseAttributes(attributes);
                        if (attrs.ContainsKey("METHOD"))
                            key.Method = attrs["METHOD"].ToUpper();
                        if (attrs.ContainsKey("URI"))
                            key.Uri = string.IsNullOrEmpty(m3u8Url) ?
                                attrs["URI"] : m3u8Url.CombineUri(attrs["URI"]);
                        if (attrs.ContainsKey("IV"))
                            key.IV = attrs["IV"];
                    }
                    continue;
                }
                match = Regex.Match(line, @"^#EXT-X-BYTERANGE:(.*)?$");
                if (match.Success)
                {
                    var val = match.Groups[1].Value;
                    if (val != "")
                    {
                        byteRange = new ByteRange();
                        match = Regex.Match(val, @"([0-9.]*)?@?([0-9.]*)?");
                        if (match.Success)
                        {
                            var length = match.Groups[1].Value;
                            if (length != "")
                                byteRange.Length = int.Parse(length);
                            var offset = match.Groups[2].Value;
                            if (offset != "")
                                byteRange.Offset = int.Parse(offset);
                        }
                    }
                    continue;
                }
                match = Regex.Match(line, @"^#EXT-X-MAP:(.*)$");
                if (match.Success)
                {
                    segmentMap = new SegmentMap();
                    var attributes = match.Groups[1].Value;
                    if (attributes != "")
                    {
                        var attrs = ParseAttributes(attributes);
                        if (attrs.ContainsKey("URI"))
                            segmentMap.Uri = string.IsNullOrEmpty(m3u8Url) ?
                                attrs["URI"] : m3u8Url.CombineUri(attrs["URI"]);

                        // Set byte range
                        if (attrs.ContainsKey("BYTERANGE"))
                        {
                            var val = attrs["BYTERANGE"];
                            var mapByteRange = new ByteRange();
                            match = Regex.Match(val, @"([0-9.]*)?@?([0-9.]*)?");
                            if (match.Success)
                            {
                                var length = match.Groups[1].Value;
                                if (length != "")
                                    mapByteRange.Length = int.Parse(length);
                                var offset = match.Groups[2].Value;
                                if (offset != "")
                                    mapByteRange.Offset = int.Parse(offset);
                            }
                            segmentMap.ByteRange = mapByteRange;
                        }

                        // Set key
                        segmentMap.Key = new SegmentKey
                        {
                            Method = key.Method,
                            Uri = key.Uri,
                            IV = key.IV
                        };
                        if (key.Method != "NONE" && key.IV == "")
                        {
                            // Undefined behavior
                            var index16 = Convert.ToString(segment.Index, 16);
                            segmentMap.Key.IV = $"0x{index16.PadLeft(32, '0')}";
                        }
                        part.SegmentMap = segmentMap;
                    }
                    continue;
                }
            }
            if (!playlist.IsM3U)
                throw new Exception("Not found EXTM3U tag.");
            return playlist;
        }
    }
}
