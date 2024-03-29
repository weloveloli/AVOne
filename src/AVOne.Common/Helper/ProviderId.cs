﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Helper
{
    public class ProviderId
    {
        public string Provider { get; set; }

        public string Id { get; set; }

        public double? Position { get; set; }

        public bool? Update { get; set; }

        public ProviderId(string provider, string id)
        {
            Provider = provider;
            Id = id;
        }

        public static ProviderId Parse(string rawPid)
        {
            var values = rawPid?.Split(':');
            var provider = values?.Length > 0 ? values[0] : string.Empty;
            var id = values?.Length > 1 ? values[1] : string.Empty;
            return new ProviderId(provider, id)
            {
                Position = values?.Length > 2 ? ToDouble(values[2]) : null,
                Update = values?.Length > 3 ? ToBool(values[3]) : null
            };
        }

        public override string ToString()
        {
            var pid = this;
            var values = new List<string>
        {
            pid.Provider, pid.Id
        };
            if (pid.Position.HasValue)
            {
                values.Add(pid.Position.Value.ToString());
            }

            if (pid.Update.HasValue)
            {
                values.Add((values.Count == 2 ? ":" : string.Empty) + pid.Update);
            }

            return string.Join(':', values);
        }

        private static bool? ToBool(string s)
        {
            switch (s)
            {
                case "1":
                case "t":
                case "T":
                case "true":
                case "True":
                case "TRUE":
                    return true;
                case "0":
                case "f":
                case "F":
                case "false":
                case "False":
                case "FALSE":
                    return false;
                default:
                    break;
            }

            return null;
        }

        private static double? ToDouble(string s)
        {
            return double.TryParse(s, out var result) ? result : null;
        }
    }
}
