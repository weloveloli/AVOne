// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Parser
{
    using System.Collections.Generic;

    public class MasterPlaylist
    {
        public bool IsM3U { get; set; }
        public List<StreamInfo> StreamInfos { get; set; } = new();
        public List<MediaGroup> MediaGroups { get; set; } = new();
        public string Manifest { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public class StreamInfo
    {
        public int Bandwidth { get; set; }
        public double FrameRate { get; set; }
        public int ProgramId { get; set; }
        public string Uri { get; set; } = "";
        public string Codecs { get; set; } = "";
        public string Audio { get; set; } = "";
        public string Video { get; set; } = "";
        public string Subtitles { get; set; } = "";
        public string Closed_Captions { get; set; } = "";
        public Resolution? Resolution { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
    }

    public class Resolution
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public enum MediaType
    {
        None,
        AUDIO,
        VIDEO,
        SUBTITLES,
        CLOSED_CAPTIONS
    }

    public class MediaGroup
    {
        public MediaType Type { get; set; }
        public string Uri { get; set; } = "";
        public string GroupId { get; set; } = "";
        public string Language { get; set; } = "";
        public string Name { get; set; } = "";
        public string Characteristics { get; set; } = "";
        public bool AutoSelect { get; set; }
        public bool Default { get; set; }
        public bool Forced { get; set; }
    }
}
