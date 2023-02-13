﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers.Official.Tests
{
    using AVOne.Enum;
    using AVOne.Providers.Official;
    using Newtonsoft.Json;

    public class TestOfficialNameResolver
    {
        [Theory]
        [InlineData("FC2PPV-3068336", "FC2-3068336", PornMovieFlags.Uncensored, MovieIdCategory.Amateur)]
        [InlineData("FC2-PPV-3068336", "FC2-3068336", PornMovieFlags.Uncensored, MovieIdCategory.Amateur)]
        [InlineData("FC2-3068336", "FC2-3068336", PornMovieFlags.Uncensored, MovieIdCategory.Amateur)]
        [InlineData("FC2-3068336 sdsdsdsds", "FC2-3068336", PornMovieFlags.Uncensored, MovieIdCategory.Amateur)]
        [InlineData("sdsdsdsds FC2-3068336", "FC2-3068336", PornMovieFlags.Uncensored, MovieIdCategory.Amateur)]
        [InlineData("FC2-3068336-C", "FC2-3068336", PornMovieFlags.Uncensored | PornMovieFlags.ChineaseSubtilte, MovieIdCategory.Amateur)]
        public void TestOfficialMovieNameParserV2Provider(string name, string expectedId, PornMovieFlags flags, MovieIdCategory movieIdCategory)
        {
            var json = File.ReadAllText(Path.Combine("files", "testconfig.json"));
            var provider = new OfficialLocalMetadataProvider(JsonConvert.DeserializeObject<TestApplicationConfigs>(json));
            var id = provider.Parse(name);
            Assert.NotNull(id);
            Assert.Equal(expectedId, id.Id);
            Assert.Equal(flags, id.Flags);
            Assert.Equal(movieIdCategory, id.Category);
            Assert.Equal(name, id.FileName);
        }

        [Fact]
        public void TestOfficialMovieNameParserV2ProviderIds()
        {
            var json = File.ReadAllText(Path.Combine("files", "testconfig.json"));
            var provider = new OfficialLocalMetadataProvider(JsonConvert.DeserializeObject<TestApplicationConfigs>(json));
            var lines = File.ReadAllLines(Path.Combine("files", "TextMovieIds.txt"));
            foreach (var line in lines)
            {
                var lineTokens = line.Split('\t');
                if (lineTokens.Length != 2)
                {
                    continue;
                }
                var name = lineTokens[0];
                var expectedId = lineTokens[1];
                var id = provider.GetId(name, out var _, out var _);
                Assert.NotNull(id);
                Assert.Equal(expectedId, id);
            }
        }
    }
}