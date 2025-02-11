// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Tests.Extractor
{
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Extractor.Embeded;
    using Xunit;

    public class EmbedDnaavExtratorTests
    {
        private string html;
        private string embedHtml;

        public EmbedDnaavExtratorTests()
        {
            this.html = File.ReadAllText(Path.Combine("websites", "dnaav.txt"));
            this.embedHtml = File.ReadAllText(Path.Combine("websites", "dnaav_embed.txt"));
        }

        [Fact()]
        public async Task SupportTest()
        {
            var extractor = new EmbedDnaavExtrator();
            Assert.True(extractor.IsEmbedUrlSupported("https://www.dnaav.com/embed/215963.html"));
            var items = await extractor.ExtractFromEmbedPageAsync("https://dnaav.com", html, "https://dnaav.com/embed/1", embedHtml);
            Assert.Single(items);
            var item = items.First();
            Assert.IsType<M3U8Item>(item);
            var m3u8Item = item as M3U8Item;
            Assert.Equal("https://v2024.ddcdnbf.com/20250124/zvXg0iKj/index.m3u8", m3u8Item.Url);
            Assert.Equal(Enum.MediaQuality.High, m3u8Item.Quality);
            Assert.Equal("无毛萝莉啪", m3u8Item.Title);
        }
    }
}
