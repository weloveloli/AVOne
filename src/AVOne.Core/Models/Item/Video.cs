// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Models.Item
{
    using AVOne.Enum;
    using AVOne.Models.Info;

    public class Video : BaseItem
    {
        protected Video()
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
    }
}
