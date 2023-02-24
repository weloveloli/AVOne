// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Parser.DashParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using AVOne.Providers.Official.Download.Extensions;

    internal class MpdParser
    {
        public Mpd Parse(string manifest, string mpdUrl = "")
        {
            var element = GetMPD(manifest);
            var mediaPresentationDuration = element.GetAttribute("mediaPresentationDuration");
            var childNodes = element.ChildNodes.AsEnumerable();
            var baseUrl = childNodes
                .FirstOrDefault(it => it.Name == "BaseURL")?.InnerText ?? "";
            var periods = childNodes
                .Where(it => it.Name == "Period")
                .Select(it => GetPeriod((XmlElement)it))
                .ToList();
            var mpd = new Mpd
            {
                Url = mpdUrl,
                Manifest = manifest,
                Periods = periods,
                BaseUrl = baseUrl,
                MediaPresentationDuration = mediaPresentationDuration != "" ?
                    mediaPresentationDuration.ParseTimeSpan() : null
            };

            mpd = ExpandSegmentTemplate(mpd);
            mpd = ExpandSegmentUrl(mpd, mpdUrl);
            return mpd;
        }

        protected XmlElement GetMPD(string manifest)
        {
            var doc = new XmlDocument();
            doc.LoadXml(manifest);
            var mpd = doc.ChildNodes.AsEnumerable()
                .Where(it =>
                    it.Name == "MPD" &&
                    it.NodeType == XmlNodeType.Element)
                .FirstOrDefault();
            return mpd == null ? throw new Exception("Not found MPD tag.") : (XmlElement)mpd;
        }

        protected Period GetPeriod(XmlElement element)
        {
            var duration = element.GetAttribute("duration");
            var childNodes = element.ChildNodes.AsEnumerable();
            var adaptationSets = childNodes
                .Where(it => it.Name == "AdaptationSet")
                .Select(it => GetAdaptationSet((XmlElement)it))
                .ToList();
            return new Period
            {
                AdaptationSets = adaptationSets,
                Duration = duration != "" ?
                    duration.ParseTimeSpan() : null
            };
        }

        protected AdaptationSet GetAdaptationSet(XmlElement element)
        {
            var lang = element.GetAttribute("lang");
            var mimeType = element.GetAttribute("mimeType");
            var contentType = element.GetAttribute("contentType");
            var segmentAlignment = element.GetAttribute("segmentAlignment");
            var childNodes = element.ChildNodes.AsEnumerable();
            var representations = childNodes
                .Where(it => it.Name == "Representation")
                .Select(it => GetRepresentation((XmlElement)it))
                .ToList();
            var baseUrl = childNodes
                .FirstOrDefault(it => it.Name == "BaseURL")?.InnerText ?? "";
            var segmentTemplate = childNodes
                .FirstOrDefault(it => it.Name == "SegmentTemplate");
            return new AdaptationSet
            {
                Representations = representations,
                SegmentTemplate = segmentTemplate != null ?
                    GetSegmentTemplate((XmlElement)segmentTemplate) : null,
                BaseUrl = baseUrl,
                Lang = lang,
                MimeType = mimeType,
                ContentType = contentType,
                SegmentAlignment = segmentAlignment != "" && (segmentAlignment.ParseBool() ?? false)
            };
        }

        protected Representation GetRepresentation(XmlElement element)
        {
            var id = element.GetAttribute("id");
            var mimeType = element.GetAttribute("mimeType");
            var contentType = element.GetAttribute("contentType");
            var codecs = element.GetAttribute("codecs");
            var bandwidth = element.GetAttribute("bandwidth");
            var width = element.GetAttribute("width");
            var height = element.GetAttribute("height");
            var audioSamplingRate = element.GetAttribute("audioSamplingRate");
            var frameRate = element.GetAttribute("frameRate");
            var startWithSAP = element.GetAttribute("startWithSAP");
            var childNodes = element.ChildNodes.AsEnumerable();
            var segmentList = childNodes
                .FirstOrDefault(it => it.Name == "SegmentList");
            var segmentTemplate = childNodes
                .FirstOrDefault(it => it.Name == "SegmentTemplate");
            var segmentBase = childNodes
                .FirstOrDefault(it => it.Name == "SegmentBase");
            var baseUrl = childNodes
                .FirstOrDefault(it => it.Name == "BaseURL")?.InnerText ?? "";
            return new Representation
            {
                SegmentList = segmentList != null ?
                    GetSegmentList((XmlElement)segmentList) : new(),
                SegmentTemplate = segmentTemplate != null ?
                    GetSegmentTemplate((XmlElement)segmentTemplate) : null,
                SegmentBase = segmentBase != null ?
                    GetSegmentBase((XmlElement)segmentBase) : null,
                BaseUrl = baseUrl,
                Id = id,
                MimeType = mimeType,
                ContentType = contentType,
                Codecs = codecs,
                FrameRate = frameRate,
                Bandwidth = bandwidth != "" ? int.Parse(bandwidth) : null,
                Width = width != "" ? int.Parse(width) : null,
                Height = height != "" ? int.Parse(height) : null,
                AudioSamplingRate = audioSamplingRate != "" ? int.Parse(audioSamplingRate) : null,
                StartWithSAP = startWithSAP != "" ? int.Parse(startWithSAP) : null
            };
        }

        protected SegmentList GetSegmentList(XmlElement element)
        {
            var duration = element.GetAttribute("duration");
            var timescale = element.GetAttribute("timescale");
            var childNodes = element.ChildNodes.AsEnumerable();
            var initialization = childNodes
                .FirstOrDefault(it => it.Name == "Initialization");
            var segmentUrls = childNodes
                .Where(it => it.Name == "SegmentURL")
                .Select(it => GetSegmentUrl((XmlElement)it))
                .ToList();
            var segmentList = new SegmentList
            {
                Initialization = initialization != null ?
                   GetInitialization((XmlElement)initialization) : null,
                SegmentUrls = segmentUrls,
                Timescale = timescale != "" ? int.Parse(timescale) : null,
                Duration = duration != "" ? int.Parse(duration) : null
            };
            foreach (var item in segmentUrls)
            {
                item.Timescale = segmentList.Timescale ?? 1;
                item.Duration = segmentList.Duration ?? 0;
            }
            return segmentList;
        }

        protected Initialization GetInitialization(XmlElement element)
        {
            var sourceURL = element.GetAttribute("sourceURL");
            var range = element.GetAttribute("range");
            return new Initialization
            {
                SourceURL = sourceURL,
                Range = range != "" ?
                    new IndexRange
                    {
                        From = long.Parse(range.Split('-')[0]),
                        To = long.Parse(range.Split('-')[1])
                    } : null
            };
        }

        protected SegmentUrl GetSegmentUrl(XmlElement element)
        {
            var media = element.GetAttribute("media");
            var index = element.GetAttribute("index");
            var mediaRange = element.GetAttribute("mediaRange");
            var indexRange = element.GetAttribute("indexRange");
            return new SegmentUrl
            {
                Media = media,
                Index = index,
                MediaRange = mediaRange != "" ?
                    new IndexRange
                    {
                        From = long.Parse(mediaRange.Split('-')[0]),
                        To = long.Parse(mediaRange.Split('-')[1])
                    } : null,
                IndexRange = indexRange != "" ?
                    new IndexRange
                    {
                        From = long.Parse(indexRange.Split('-')[0]),
                        To = long.Parse(indexRange.Split('-')[1])
                    } : null
            };
        }

        protected SegmentTemplate GetSegmentTemplate(XmlElement element)
        {
            var initialization = element.GetAttribute("initialization");
            var media = element.GetAttribute("media");
            var startNumber = element.GetAttribute("startNumber");
            var duration = element.GetAttribute("duration");
            var timescale = element.GetAttribute("timescale");
            var childNodes = element.ChildNodes.AsEnumerable();
            var segmentTimeline = childNodes
                .FirstOrDefault(it => it.Name == "SegmentTimeline");
            return new SegmentTemplate
            {
                SegmentTimelines = segmentTimeline != null ?
                    GetSegmentTimelines((XmlElement)segmentTimeline) : new(),
                Initialization = initialization,
                Media = media,
                StartNumber = startNumber != "" ? int.Parse(startNumber) : null,
                Timescale = timescale != "" ? int.Parse(timescale) : null,
                Duration = duration != "" ? int.Parse(duration) : null
            };
        }

        protected List<SegmentTimeline> GetSegmentTimelines(XmlElement element)
        {
            static SegmentTimeline GetSegmentTimeline(XmlElement element)
            {
                var t = element.GetAttribute("t");
                var d = element.GetAttribute("d");
                var r = element.GetAttribute("r");
                return new SegmentTimeline
                {
                    T = t != "" ? int.Parse(t) : null,
                    D = d != "" ? int.Parse(d) : null,
                    R = r != "" ? int.Parse(r) : null
                };
            }
            var childNodes = element.ChildNodes.AsEnumerable();
            return childNodes
                .Where(it => it.Name == "S")
                .Select(it => GetSegmentTimeline((XmlElement)it))
                .ToList();
        }

        protected SegmentBase GetSegmentBase(XmlElement element)
        {
            var indexRange = element.GetAttribute("indexRange");
            var childNodes = element.ChildNodes.AsEnumerable();
            var initialization = childNodes
                .FirstOrDefault(it => it.Name == "Initialization");
            return new SegmentBase
            {
                Initialization = initialization != null ?
                   GetInitialization((XmlElement)initialization) : null,
                IndexRange = indexRange != "" ?
                    new IndexRange
                    {
                        From = long.Parse(indexRange.Split('-')[0]),
                        To = long.Parse(indexRange.Split('-')[1])
                    } : null
            };
        }

        protected Mpd ExpandSegmentUrl(Mpd mpd, string mpdUrl)
        {
            var baseUrl = mpdUrl;
            if (!string.IsNullOrEmpty(mpd.BaseUrl))
            {
                baseUrl = string.IsNullOrEmpty(baseUrl) ?
                    mpd.BaseUrl : baseUrl.CombineUri(mpd.BaseUrl);
                mpd.BaseUrl = baseUrl;
            }

            foreach (var period in mpd.Periods)
            {
                foreach (var adaptationSet in period.AdaptationSets)
                {
                    var adaBaseUrl = baseUrl;
                    if (!string.IsNullOrEmpty(adaptationSet.BaseUrl))
                    {
                        adaBaseUrl = string.IsNullOrEmpty(adaBaseUrl) ?
                            adaptationSet.BaseUrl : adaBaseUrl.CombineUri(adaptationSet.BaseUrl);
                        adaptationSet.BaseUrl = adaBaseUrl;
                    }

                    foreach (var representation in adaptationSet.Representations)
                    {
                        var repBaseUrl = adaBaseUrl;
                        if (!string.IsNullOrEmpty(representation.BaseUrl))
                        {
                            repBaseUrl = string.IsNullOrEmpty(repBaseUrl) ?
                                representation.BaseUrl : repBaseUrl.CombineUri(representation.BaseUrl);
                            representation.BaseUrl = repBaseUrl;
                        }

                        var segmentTemplate = representation.SegmentTemplate;
                        if (segmentTemplate != null)
                        {
                            var initUrl = repBaseUrl;
                            if (!string.IsNullOrEmpty(segmentTemplate.Initialization))
                            {
                                initUrl = string.IsNullOrEmpty(initUrl) ?
                                    segmentTemplate.Initialization : initUrl.CombineUri(segmentTemplate.Initialization);
                                segmentTemplate.Initialization = initUrl;
                            }
                            var mediaUrl = repBaseUrl;
                            if (!string.IsNullOrEmpty(segmentTemplate.Media))
                            {
                                mediaUrl = string.IsNullOrEmpty(mediaUrl) ?
                                    segmentTemplate.Media : mediaUrl.CombineUri(segmentTemplate.Media);
                                segmentTemplate.Media = mediaUrl;
                            }
                        }

                        var segmentBase = representation.SegmentBase;
                        if (segmentBase != null)
                        {
                            if (repBaseUrl != mpdUrl)
                            {
                                segmentBase.BaseUrl = repBaseUrl;
                            }
                        }

                        var segmentList = representation.SegmentList;
                        if (segmentList.Initialization != null)
                        {
                            var initialization = segmentList.Initialization;
                            var initUrl = repBaseUrl;
                            if (!string.IsNullOrEmpty(initialization.SourceURL))
                            {
                                initUrl = string.IsNullOrEmpty(initUrl) ?
                                    initialization.SourceURL : initUrl.CombineUri(initialization.SourceURL);
                            }

                            initialization.SourceURL = initUrl;
                        }
                        foreach (var segmentUrl in segmentList.SegmentUrls)
                        {
                            var mediaUrl = repBaseUrl;
                            if (!string.IsNullOrEmpty(segmentUrl.Media))
                            {
                                mediaUrl = string.IsNullOrEmpty(mediaUrl) ?
                                    segmentUrl.Media : mediaUrl.CombineUri(segmentUrl.Media);
                            }

                            segmentUrl.Media = mediaUrl;

                            var indexUrl = repBaseUrl;
                            if (!string.IsNullOrEmpty(segmentUrl.Index))
                            {
                                indexUrl = string.IsNullOrEmpty(indexUrl) ?
                                    segmentUrl.Index : indexUrl.CombineUri(segmentUrl.Index);
                                segmentUrl.Index = indexUrl;
                            }
                            else if (segmentUrl.IndexRange != null)
                            {
                                segmentUrl.Index = indexUrl;
                            }
                        }
                    }
                }
            }
            return mpd;
        }

        protected Mpd ExpandSegmentTemplate(Mpd mpd)
        {
            foreach (var period in mpd.Periods)
            {
                var periodDuration = period.Duration?.TotalSeconds ??
                    mpd.MediaPresentationDuration?.TotalSeconds ?? 0;

                foreach (var adaptationSet in period.AdaptationSets)
                {
                    var adaSegmentTemplate = adaptationSet.SegmentTemplate;

                    foreach (var representation in adaptationSet.Representations)
                    {
                        var id = representation.Id;
                        var segmentList = representation.SegmentList;

                        var segmentTemplate = representation.SegmentTemplate ?? adaSegmentTemplate;
                        if (segmentTemplate != null)
                        {
                            if (!string.IsNullOrEmpty(segmentTemplate.Initialization))
                            {
                                segmentList.Initialization = new Initialization
                                {
                                    SourceURL = segmentTemplate.Initialization
                                        .Replace("$RepresentationID$", id)
                                };
                            }

                            var totalNumber = 0;
                            var startNumber = segmentTemplate.StartNumber ?? 0;

                            var segmentTimelines = segmentTemplate.SegmentTimelines;
                            if (segmentTimelines.Count > 0)
                            {
                                var time = 0;
                                foreach (var segmentTimeline in segmentTimelines)
                                {
                                    time = segmentTimeline.T ?? time;
                                    var d = segmentTimeline.D ?? 0;
                                    var r = segmentTimeline.R ?? 0;
                                    for (var i = 0; i < r + 1; i++)
                                    {
                                        var segmentUrl = new SegmentUrl
                                        {
                                            Media = segmentTemplate.Media
                                                .Replace("$RepresentationID$", id)
                                                .Replace("$Time$", $"{time}"),
                                            Timescale = segmentTemplate.Timescale ?? 1,
                                            Duration = d
                                        };
                                        segmentList.SegmentUrls.Add(segmentUrl);
                                        time += d;
                                    }
                                }
                            }
                            else
                            {
                                if (segmentTemplate.Duration != null)
                                {
                                    var timescale = segmentTemplate.Timescale ?? 1;
                                    var duration = (double)segmentTemplate.Duration.Value / timescale;
                                    totalNumber = (int)Math.Ceiling(periodDuration / duration);
                                }
                                for (var i = startNumber; i < startNumber + totalNumber; i++)
                                {
                                    var segmentUrl = new SegmentUrl
                                    {
                                        Media = segmentTemplate.Media
                                            .Replace("$RepresentationID$", id)
                                            .Replace("$Number$", $"{i}"),
                                        Timescale = segmentTemplate.Timescale ?? 1,
                                        Duration = segmentTemplate.Duration ?? 0
                                    };
                                    segmentList.SegmentUrls.Add(segmentUrl);
                                }
                            }
                        }
                    }
                }
            }
            return mpd;
        }
    }
}
