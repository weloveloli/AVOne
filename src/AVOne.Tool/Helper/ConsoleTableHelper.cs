// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Helper
{
    using AVOne.Extensions;
    using System.Reflection;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using ConsoleTables;

    public static class ConsoleTableHelper
    {
        private const int Max_Length = 35;
        public static string[] BaseItemIncludeProperties = new string[] { "Name", "Tagline", "OriginalTitle", "Overview", "OfficialRating", "PremiereDate", "ProductionYear", "Genres", "ProviderIds", "CommunityRating", "Studios", "Tags", "People", "ItemImageInfo" };
        public static string[] PornMovieInfoIncludeProperties = new string[] { "Category", "Flags", "Id" };

        public static ConsoleTable? ToTable(BaseItem? item, IProvider provider)
        {
            if (item is null)
            {
                return null;
            }
            var table = new ConsoleTable("Key", "Value");
            table.Configure(o => o.NumberAlignment = Alignment.Left);
            table.AddRow("Provider", provider.Name);
            // get all included properties
            var properties = item.GetType().GetProperties().Where(e => BaseItemIncludeProperties.Contains(e.Name));
            AddPropertyValues(item, table, properties);

            if (item is PornMovie movie)
            {
                var pornMovieInfo = movie.PornMovieInfo;
                if (pornMovieInfo != null)
                {
                    // get all properties of PornMovieInfo
                    var pornMovieInfoProperties = pornMovieInfo.GetType().GetProperties().Where(e => PornMovieInfoIncludeProperties.Contains(e.Name)); ;
                    // add value for each properties
                    AddPropertyValues(pornMovieInfo, table, pornMovieInfoProperties, keyPrefix: "PornMovieInfo.");
                }
            }

            return table;
        }

        private static void AddPropertyValues(object o, ConsoleTable table, IEnumerable<PropertyInfo> properties, string? keyPrefix = null)
        {
            keyPrefix ??= string.Empty;
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
                if (value is string str)
                {
                    table.AddRow(keyPrefix + property.Name, str.Ellipsis(Max_Length));
                }
                // if value is IEnumerable, add to result
                else if (value is IEnumerable<object>)
                {
                    var enumerable = value as IEnumerable<object>;
                    foreach (var (item, index) in enumerable.Select((value, i) => (value, i)))
                    {
                        if (item is null)
                        {
                            continue;
                        }
                        if (index == 0)
                        {
                            table.AddRow(keyPrefix + property.Name, item.ToString().Ellipsis(Max_Length));
                        }
                        else
                        {

                            table.AddRow(string.Empty, item.ToString().Ellipsis(Max_Length));
                        }
                    }
                }
                // if value is IDictionary, add to result
                else if (value is Dictionary<string, string> dict)
                {
                    var enumerable = dict.AsEnumerable();
                    foreach (var (item, index) in enumerable.Select((value, i) => (value, i)))
                    {
                        if (index == 0)
                        {
                            table.AddRow(keyPrefix + property.Name, $"{item.Key}:{item.Value}".Ellipsis(Max_Length));
                        }
                        else
                        {

                            table.AddRow(string.Empty, $"{item.Key}:{item.Value}".Ellipsis(Max_Length));
                        }
                    }
                }
                // if value is not string or IEnumerable, add to result
                else
                {
                    table.AddRow(keyPrefix + property.Name, value.ToString().Ellipsis(Max_Length));
                }
            }
        }
    }
}
