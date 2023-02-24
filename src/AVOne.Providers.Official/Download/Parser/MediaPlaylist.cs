// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Parser
{
    using System.Collections.Generic;

    public class MediaPlaylist
    {
        public bool IsM3U { get; set; }
        public bool EndList { get; set; }
        public long MediaSequence { get; set; }
        public int DiscontinuitySequence { get; set; }
        public int TargetDuration { get; set; }
        public double TotalDuration { get; set; }
        public int Version { get; set; }
        public string PlaylistType { get; set; } = "";
        public List<Part> Parts { get; set; } = new();
        public string Manifest { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public class Part
    {
        public List<Segment> Segments { get; set; } = new();
        public SegmentMap? SegmentMap { get; set; }
        public int PartIndex { get; set; }
    }

    public class Segment
    {
        public long Index { get; set; }
        public double Duration { get; set; }
        public string Uri { get; set; } = "";
        public string Title { get; set; } = "";
        public bool Discontinuity { get; set; }
        public SegmentKey Key { get; set; } = new();
        public ByteRange? ByteRange { get; set; }
    }

    public class SegmentKey
    {
        public string Method { get; set; } = "NONE";
        public string Uri { get; set; } = "";
        public string IV { get; set; } = "";
    }

    public class ByteRange
    {
        public int Length { get; set; }
        public int? Offset { get; set; }
    }

    public class SegmentMap
    {
        public string Uri { get; set; } = "";
        public ByteRange? ByteRange { get; set; }
        public SegmentKey Key { get; set; } = new();
    }
}
