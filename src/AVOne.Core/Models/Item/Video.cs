// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Models.Item
{
    using System.Text.Json.Serialization;
    using AVOne.Enum;
    using AVOne.Models.Info;

    public class Video : BaseItem
    {
        public Video()
        {
            Tags = Array.Empty<string>();
            Genres = Array.Empty<string>();
            Studios = Array.Empty<string>();
            ExtraIds = Array.Empty<Guid>();
            SubtitleFiles = Array.Empty<string>();
            AdditionalParts = Array.Empty<string>();
        }

        /// <summary>
        /// Gets or sets the type of the video.
        /// </summary>
        /// <value>The type of the video.</value>
        public VideoType VideoType { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public Guid[] ExtraIds { get; set; }

        public virtual bool IsHD => Height >= 720;

        /// <summary>
        /// Gets or sets the subtitle paths.
        /// </summary>
        /// <value>The subtitle paths.</value>
        public string[] SubtitleFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has subtitles.
        /// </summary>
        /// <value><c>true</c> if this instance has subtitles; otherwise, <c>false</c>.</value>
        public bool HasSubtitles { get; set; }

        public string[] AdditionalParts { get; set; }
        public string CollectionName { get; set; }

        public override ItemLookupInfo GetLookupInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsPlaceHolder { get; set; }

        public string[] LocalAlternateVersions { get; set; }

        [JsonIgnore]
        public bool IsStacked => AdditionalParts.Length > 0;

        [JsonIgnore]
        public override string ContainingFolderPath
        {
            get
            {
                if (IsStacked)
                {
                    return System.IO.Path.GetDirectoryName(Path);
                }

                if (!IsPlaceHolder)
                {
                    if (VideoType == VideoType.BluRay || VideoType == VideoType.Dvd)
                    {
                        return Path;
                    }
                }

                return base.ContainingFolderPath;
            }
        }
    }
}
