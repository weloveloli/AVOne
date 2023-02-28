// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8.Parser
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using AVOne.Providers.Official.Downloader.M3U8.Extensions;

    internal class MasterPlaylistParser : BaseParser
    {
        // Regex refer to:
        // https://github.com/videojs/m3u8-parser
        public MasterPlaylist Parse(string manifest, string m3u8Url = "")
        {
            using (var reader = new StringReader(manifest))
            {
                var playlist = new MasterPlaylist();
                playlist.Url = m3u8Url;
                playlist.Manifest = manifest;

                var streamInfo = new StreamInfo();

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
                        streamInfo.Uri = string.IsNullOrEmpty(m3u8Url) ?
                            line : m3u8Url.CombineUri(line);
                        playlist.StreamInfos.Add(streamInfo);
                        streamInfo = new StreamInfo();
                        continue;
                    }

                    var match = Regex.Match(line, @"^#EXTM3U");
                    if (match.Success)
                    {
                        playlist.IsM3U = true;
                        continue;
                    }
                    match = Regex.Match(line, @"^#EXT-X-STREAM-INF:(.*)$");
                    if (match.Success)
                    {
                        var attributes = match.Groups[1].Value;
                        if (attributes != "")
                        {
                            var attrs = ParseAttributes(attributes);
                            if (attrs.ContainsKey("RESOLUTION"))
                            {
                                var val = attrs["RESOLUTION"];
                                var resolution = new Resolution();
                                var split = val.Split('x');
                                if (!string.IsNullOrEmpty(split[0]))
                                    resolution.Width = int.Parse(split[0]);
                                if (!string.IsNullOrEmpty(split[0]))
                                    resolution.Height = int.Parse(split[1]);
                                streamInfo.Resolution = resolution;
                            }
                            if (attrs.ContainsKey("PROGRAM-ID"))
                                streamInfo.ProgramId = int.Parse(attrs["PROGRAM-ID"]);
                            if (attrs.ContainsKey("BANDWIDTH"))
                                streamInfo.Bandwidth = int.Parse(attrs["BANDWIDTH"]);
                            if (attrs.ContainsKey("FRAME-RATE"))
                                streamInfo.FrameRate = double.Parse(attrs["FRAME-RATE"]);
                            if (attrs.ContainsKey("CODECS"))
                                streamInfo.Codecs = attrs["CODECS"];
                            if (attrs.ContainsKey("AUDIO"))
                                streamInfo.Audio = attrs["AUDIO"];
                            if (attrs.ContainsKey("VIDEO"))
                                streamInfo.Video = attrs["VIDEO"];
                            if (attrs.ContainsKey("SUBTITLES"))
                                streamInfo.Subtitles = attrs["SUBTITLES"];
                            if (attrs.ContainsKey("CLOSED-CAPTIONS"))
                                streamInfo.Closed_Captions = attrs["CLOSED-CAPTIONS"];
                            streamInfo.Attributes = attrs;
                        }
                        continue;
                    }
                    match = Regex.Match(line, @"^#EXT-X-MEDIA:(.*)$");
                    if (match.Success)
                    {
                        var mediaGroup = new MediaGroup();
                        var attributes = match.Groups[1].Value;
                        if (attributes != "")
                        {
                            var attrs = ParseAttributes(attributes);
                            if (attrs.ContainsKey("TYPE"))
                            {
                                var val = attrs["TYPE"];
                                if (val == "AUDIO")
                                    mediaGroup.Type = MediaType.AUDIO;
                                if (val == "VIDEO")
                                    mediaGroup.Type = MediaType.VIDEO;
                                if (val == "SUBTITLES")
                                    mediaGroup.Type = MediaType.SUBTITLES;
                                if (val == "CLOSED-CAPTIONS")
                                    mediaGroup.Type = MediaType.CLOSED_CAPTIONS;
                            }
                            if (attrs.ContainsKey("GROUP-ID"))
                            {
                                mediaGroup.GroupId = attrs["GROUP-ID"];
                            }
                            if (attrs.ContainsKey("LANGUAGE"))
                            {
                                mediaGroup.Language = attrs["LANGUAGE"];
                            }
                            if (attrs.ContainsKey("NAME"))
                            {
                                mediaGroup.Name = attrs["NAME"];
                            }
                            if (attrs.ContainsKey("URI"))
                            {
                                mediaGroup.Uri = string.IsNullOrEmpty(m3u8Url) ?
                                    attrs["URI"] : m3u8Url.CombineUri(attrs["URI"]);
                            }
                            if (attrs.ContainsKey("AUTOSELECT"))
                            {
                                var val = attrs["AUTOSELECT"];
                                if (val == "YES")
                                    mediaGroup.AutoSelect = true;
                            }
                            if (attrs.ContainsKey("DEFAULT"))
                            {
                                var val = attrs["DEFAULT"];
                                if (val == "YES")
                                    mediaGroup.Default = true;
                            }
                            if (attrs.ContainsKey("FORCED"))
                            {
                                var val = attrs["FORCED"];
                                if (val == "YES")
                                    mediaGroup.Forced = true;
                            }
                            if (attrs.ContainsKey("CHARACTERISTICS"))
                            {
                                mediaGroup.Characteristics = attrs["CHARACTERISTICS"];
                            }
                        }
                        playlist.MediaGroups.Add(mediaGroup);
                        continue;
                    }
                }
                if (!playlist.IsM3U)
                    throw new Exception("Not found EXTM3U tag.");
                return playlist;
            }
        }
    }
}
