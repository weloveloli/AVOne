// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Models.Item
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using AVOne.Helper;
    using AVOne.Models.Info;

    public abstract class MetaDataItem
    {
        protected MetaDataItem()
        {
            Tags = Array.Empty<string>();
            Genres = Array.Empty<string>();
            Studios = Array.Empty<string>();
            ImageInfos = Array.Empty<ItemImageInfo>();
            People = new List<PersonInfo>();
        }

        /// <summary>
        /// Gets or sets the studios.
        /// </summary>
        /// <value>The studios.</value>
        public string[] Studios { get; set; }

        /// <summary>
        /// Gets or sets the genres.
        /// </summary>
        /// <value>The genres.</value>
        public string[] Genres { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public string[] Tags { get; set; }

        /// <summary>
        /// Gets or sets the overview.
        /// </summary>
        /// <value>The overview.</value>
        public string? Overview { get; set; }

        /// <summary>
        /// Gets or sets the custom rating.
        /// </summary>
        /// <value>The custom rating.</value>
        public string? CustomRating { get; set; }

        /// <summary>
        /// Gets or sets the critic rating.
        /// </summary>
        /// <value>The critic rating.</value>
        public float? CriticRating { get; set; }

        /// <summary>
        /// Gets or sets the official rating.
        /// </summary>
        /// <value>The official rating.</value>
        public string? OfficialRating { get; set; }

        /// <summary>
        /// Gets or sets the community rating.
        /// </summary>
        /// <value>The community rating.</value>
        public float? CommunityRating { get; set; }
        public virtual ItemImageInfo[] ImageInfos { get; set; }
        public List<PersonInfo> People { get; set; }

        /// <summary>
        /// Gets or sets the home page URL.
        /// </summary>
        /// <value>The home page URL.</value>
        public string? HomePageUrl { get; set; }

        /// <summary>
        /// Gets or sets the production year.
        /// </summary>
        /// <value>The production year.</value>
        public int? ProductionYear { get; set; }

        public void AddImage(ItemImageInfo image)
        {
            var current = ImageInfos;
            var currentCount = current.Length;
            var newArr = new ItemImageInfo[currentCount + 1];
            current.CopyTo(newArr, 0);
            newArr[currentCount] = image;
            ImageInfos = newArr;
        }
        public void AddPerson(PersonInfo p)
        {
            People ??= new List<PersonInfo>();

            PeopleHelper.AddPerson(People, p);
        }
    }
}
