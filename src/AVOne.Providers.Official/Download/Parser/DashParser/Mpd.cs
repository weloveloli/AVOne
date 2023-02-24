// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Parser.DashParser
{
    using System;
    using System.Collections.Generic;

    public class Mpd
    {
        public List<Period> Periods { get; set; } = new();
        public string BaseUrl { get; set; } = "";
        public TimeSpan? MediaPresentationDuration { get; set; }
        public string Manifest { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public class Period
    {
        public List<AdaptationSet> AdaptationSets { get; set; } = new();
        public TimeSpan? Duration { get; set; }
    }

    public class AdaptationSet
    {
        public List<Representation> Representations { get; set; } = new();
        public SegmentTemplate? SegmentTemplate { get; set; }
        public string BaseUrl { get; set; } = "";
        public string Lang { get; set; } = "";
        public string MimeType { get; set; } = "";
        public string ContentType { get; set; } = "";
        public bool SegmentAlignment { get; set; }
    }

    public class Representation
    {
        public SegmentList SegmentList { get; set; } = new();
        public SegmentTemplate? SegmentTemplate { get; set; }
        public SegmentBase? SegmentBase { get; set; }
        public string BaseUrl { get; set; } = "";
        public string Id { get; set; } = "";
        public string MimeType { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string Codecs { get; set; } = "";
        public string FrameRate { get; set; } = "";
        public int? Bandwidth { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? AudioSamplingRate { get; set; }
        public int? StartWithSAP { get; set; }
    }

    public class SegmentList
    {
        public Initialization? Initialization { get; set; }
        public List<SegmentUrl> SegmentUrls { get; set; } = new();
        public int? Timescale { get; set; }
        public int? Duration { get; set; }
    }

    public class Initialization
    {
        public string SourceURL { get; set; } = "";
        public IndexRange? Range { get; set; }
    }

    public class SegmentUrl
    {
        public string Media { get; set; } = "";
        public string Index { get; set; } = "";
        public IndexRange? MediaRange { get; set; }
        public IndexRange? IndexRange { get; set; }
        public int Timescale { get; set; }
        public int Duration { get; set; }
    }

    public class IndexRange
    {
        public long From { get; set; }
        public long To { get; set; }
    }

    public class SegmentTemplate
    {
        public List<SegmentTimeline> SegmentTimelines { get; set; } = new();
        public string Initialization { get; set; } = "";
        public string Media { get; set; } = "";
        public int? StartNumber { get; set; }
        public int? Timescale { get; set; }
        public int? Duration { get; set; }
    }

    public class SegmentTimeline
    {
        public int? T { get; set; }
        public int? D { get; set; }
        public int? R { get; set; }
    }

    public class SegmentBase
    {
        public string BaseUrl { get; set; } = "";
        public Initialization? Initialization { get; set; }
        public IndexRange? IndexRange { get; set; }
    }
}
