// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Extensions
{
    using System.Reflection;
    using AVOne.Configuration;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers;
    using MediaBrowser.Model.Dto;

    public static class MetadataResultExtensions
    {
        public static string[] IncludeProperties = new string[] { "Name", "Tagline", "OriginalTitle", "Overview", "OfficialRating", "PremiereDate", "ProductionYear", "Genres", "ProviderIds", "CommunityRating", "Studios", "Tags", "People", "ItemImageInfo" };

        public static List<NameValue>? NameValues<T>(this MetadataResult<T> metadataResult, IProvider provider) where T : BaseItem
        {
            // return null when metadataResult is null or HasMetadata is false
            if (metadataResult == null || !metadataResult.HasMetadata)
            {
                return null;
            }

            // return null when metadataResult.Item is null
            var result = new List<NameValue>();
            if (metadataResult.Item == null)
            {
                return result;
            }
            result.Add(new NameValue("Provider", provider.Name));
            // get all included properties
            var properties = metadataResult.Item.GetType().GetProperties().Where(e => IncludeProperties.Contains(e.Name));
            AddPropertyValues(metadataResult.Item, result, properties);

            if (metadataResult.Item is PornMovie)
            {
                var movie = metadataResult.Item as PornMovie;
                var pornMovieInfo = movie.PornMovieInfo;
                if (pornMovieInfo != null)
                {
                    // get all properties of PornMovieInfo
                    var pornMovieInfoProperties = pornMovieInfo.GetType().GetProperties();
                    // add value for each properties
                    AddPropertyValues(pornMovieInfo, result, pornMovieInfoProperties);
                }
            }

            return result;
        }

        private static void AddPropertyValues(object o, List<NameValue> result, IEnumerable<PropertyInfo> properties)
        {
            foreach (var property in properties)
            {
                // get property value
                var value = property.GetValue(o);
                // if value is null, continue
                if (value == null)
                {
                    continue;
                }
                // if value is string, add to result
                if (value is string)
                {
                    result.Add(new NameValue(property.Name, value.ToString()));
                }
                // if value is IEnumerable, add to result
                else if (value is IEnumerable<object>)
                {
                    var enumerable = value as IEnumerable<object>;
                    foreach (var item in enumerable)
                    {
                        result.Add(new NameValue(property.Name, item.ToString()));
                    }
                }
                // if value is not string or IEnumerable, add to result
                else
                {
                    result.Add(new NameValue(property.Name, value.ToString()));
                }
            }
        }
    }
}
