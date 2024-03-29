﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        private static readonly Regex WebUrlExpression = new(@"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Singleline | RegexOptions.Compiled);

        [DebuggerStepThrough]
        public static bool IsWebUrl(this string target)
        {
            return !string.IsNullOrEmpty(target) && WebUrlExpression.IsMatch(target);
        }

        public static string TrimStart(this string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString))
            {
                return target;
            }

            var result = target;
            while (result.StartsWith(trimString, StringComparison.OrdinalIgnoreCase))
            {
                result = result[trimString.Length..];
            }

            return result;
        }

        public static string TrimEnd(this string target, params string[]? trimStrings)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException($"{nameof(target)} can not be null or blank.", nameof(target));
            }

            trimStrings = trimStrings?.Where(o => string.IsNullOrEmpty(o) == false).Distinct().ToArray();
            if (trimStrings?.Any() != true)
            {
                return target;
            }

            var found = false;

            do
            {
                found = false;
                foreach (var trimString in trimStrings)
                {
                    while (target.EndsWith(trimString, StringComparison.OrdinalIgnoreCase))
                    {
                        target = target[..^trimString.Length];
                        found = true;
                    }
                }
            } while (found);
            return target;
        }

        public static string Trim(this string target, string trimString)
            => target.TrimStart(trimString).TrimEnd(trimString);

        /// <summary>
        /// Strips the HTML.
        /// </summary>
        /// <param name="htmlString">The HTML string.</param>
        /// <returns><see cref="string" />.</returns>
        public static string StripHtml(this string htmlString)
        {
            // http://stackoverflow.com/questions/1349023/how-can-i-strip-html-from-text-in-net
            const string Pattern = @"<(.|\n)*?>";

            return Regex.Replace(htmlString, Pattern, string.Empty).Trim();
        }

        /// <summary>
        /// Gets the Md5.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns><see cref="Guid" />.</returns>
        public static Guid GetMD5(this string str)
        {
#pragma warning disable CA5351
            using var provider = MD5.Create();
            return new Guid(provider.ComputeHash(Encoding.Unicode.GetBytes(str)));

#pragma warning restore CA5351
        }

        public static string? Ellipsis(this string? str, int number)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            else if (str.Length > number)
            {
                str = $"{str.Substring(0, number)}...";
            }
            return str;
        }

        /// <summary>
        /// Returns the part on the left of the <c>needle</c>.
        /// </summary>
        /// <param name="haystack">The string to seek.</param>
        /// <param name="needle">The needle to find.</param>
        /// <returns>The part left of the <paramref name="needle" />.</returns>
        public static ReadOnlySpan<char> LeftPart(this ReadOnlySpan<char> haystack, char needle)
        {
            var pos = haystack.IndexOf(needle);
            return pos == -1 ? haystack : haystack[..pos];
        }

        /// <summary>
        /// Returns the part on the right of the <c>needle</c>.
        /// </summary>
        /// <param name="haystack">The string to seek.</param>
        /// <param name="needle">The needle to find.</param>
        /// <returns>The part right of the <paramref name="needle" />.</returns>
        public static ReadOnlySpan<char> RightPart(this ReadOnlySpan<char> haystack, char needle)
        {
            var pos = haystack.LastIndexOf(needle);
            if (pos == -1)
            {
                return haystack;
            }

            if (pos == haystack.Length - 1)
            {
                return ReadOnlySpan<char>.Empty;
            }

            return haystack[(pos + 1)..];
        }

        public static string ToStringInvariant<T>(this T value)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        // add function to escape the string to be a valid filename
        public static string EscapeFileName(this string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
