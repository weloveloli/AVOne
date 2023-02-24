// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    public static class XmlExtension
    {
        public static IEnumerable<XmlNode> AsEnumerable(this XmlNodeList xmlNodeList)
        {
            foreach (XmlNode item in xmlNodeList)
            {
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        public static TimeSpan? ParseTimeSpan(this string val)
        {
            return val == null ? null : XmlConvert.ToTimeSpan(val);
        }

        public static bool? ParseBool(this string val)
        {
            return val == null ? null : bool.Parse(val);
        }
    }
}
