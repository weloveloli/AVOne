// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Extensions
{
    using System;
    using AVOne.Enum;
    using AVOne.Models.Info;
    using AVOne.Models.Item;

    public static class BaseItemExtensions
    {
        /// <summary>
        /// Adds a studio to the item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">Throws if name is null.</exception>
        public static void AddStudio(this BaseItem item, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var current = item.Studios;

            if (!current.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                var curLen = current.Length;
                if (curLen == 0)
                {
                    item.Studios = new[] { name };
                }
                else
                {
                    var newArr = new string[curLen + 1];
                    current.CopyTo(newArr, 0);
                    newArr[curLen] = name;
                    item.Studios = newArr;
                }
            }
        }

        /// <summary>
        /// Adds the trailer URL.
        /// </summary>
        /// <param name="item">Media item.</param>
        /// <param name="url">Trailer URL.</param>
        public static void AddTrailerUrl(this BaseItem item, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var current = item.RemoteTrailers.FirstOrDefault(i => string.Equals(i.Url, url, StringComparison.OrdinalIgnoreCase));

            if (current == null)
            {
                var mediaUrl = new MediaUrl
                {
                    Url = url
                };

                if (item.RemoteTrailers.Count == 0)
                {
                    item.RemoteTrailers = new[] { mediaUrl };
                }
                else
                {
                    var oldIds = item.RemoteTrailers;
                    var newIds = new MediaUrl[oldIds.Count + 1];
                    oldIds.CopyTo(newIds);
                    newIds[oldIds.Count] = mediaUrl;
                    item.RemoteTrailers = newIds;
                }
            }
        }

        /// <summary>
        /// Adds a genre to the item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">Throwns if name is null.</exception>
        public static void AddGenre(this BaseItem item, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var genres = item.Genres;
            if (!genres.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                var list = genres.ToList();
                list.Add(name);
                item.Genres = list.ToArray();
            }
        }

        public static void AddTag(this BaseItem item, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var current = item.Tags;

            if (!current.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                item.Tags = current.Length == 0 ? (new[] { name }) : current.Concat(new[] { name }).ToArray();
            }
        }

        /// <summary>
        /// Gets the image path.
        /// </summary>
        /// <param name="imageType">Type of the image.</param>
        /// <param name="imageIndex">Index of the image.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="ArgumentNullException">Item is null.</exception>
        public static string GetImagePath(this BaseItem item, ImageType imageType, int imageIndex)
#pragma warning disable CS8603 // Possible null reference return.
            => GetImageInfo(item, imageType, imageIndex)?.Path;
#pragma warning restore CS8603 // Possible null reference return.

        /// <summary>
        /// Gets the image information.
        /// </summary>
        /// <param name="imageType">Type of the image.</param>
        /// <param name="imageIndex">Index of the image.</param>
        /// <returns>ItemImageInfo.</returns>
        public static ItemImageInfo GetImageInfo(this BaseItem item, ImageType imageType, int imageIndex)
        {
            if (imageType == ImageType.Chapter)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }

#pragma warning disable CS8603 // Possible null reference return.
            return GetImages(item, imageType)
                .ElementAtOrDefault(imageIndex);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static IEnumerable<ItemImageInfo> GetImages(this BaseItem item, ImageType imageType)
        {
            if (imageType == ImageType.Chapter)
            {
                throw new ArgumentException("No image info for chapter images");
            }

            // Yield return is more performant than LINQ Where on an Array
            for (var i = 0; i < item.ImageInfos.Length; i++)
            {
                var imageInfo = item.ImageInfos[i];
                if (imageInfo.Type == imageType)
                {
                    yield return imageInfo;
                }
            }
        }
    }
}
