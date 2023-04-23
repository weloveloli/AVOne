// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Tests
{
    using System.Text.RegularExpressions;
    using AVOne.Providers.Official.Extractor;
    using Moq;
    using Xunit;

    public class AV51ClubExtratorTests
    {
        static string ExtractPlayerJsonString(string input, string playerName)
        {
            string pattern = "<script type=\"text\\/javascript\">var " + playerName + "=(\\{.+\\})<\\/script>";
            RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;
            Regex regex = new Regex(pattern, options);
            System.Text.RegularExpressions.Match match = regex.Match(input);
            return match.Groups[1].Value;
        }
        [Fact()]
        public void AV51ClubExtratorTest()
        {
            var html = File.ReadAllText(Path.Combine("websites", "51av.txt"));
            string playerName = "player_aaaa";
            string jsonString = ExtractPlayerJsonString(html, playerName);
            var source = new AV51ClubExtrator(null, null, new Mock<IHttpClientFactory>().Object).GetM3U8Sources(html);

            Assert.Single(source);

            Assert.Contains("GACHI-326", AV51ClubExtrator.GetStringFromHtml(html, AV51ClubExtrator.TitleRegex()));
        }
    }
}

