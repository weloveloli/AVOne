// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Test
{
    using AutoFixture;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Models.Item;
    using Moq;
    using Newtonsoft.Json;

    public class PornMovidInfoTest : BaseTestCase
    {

        public PornMovidInfoTest()
        {
            var json = File.ReadAllText(Path.Combine("files", "testconfig.json"));
            var mockManager = fixture.Freeze<Mock<IConfigurationManager>>();
            var config = JsonConvert.DeserializeObject<TestApplicationConfigs>(json);
            mockManager.Setup(e => e.CommonConfiguration).Returns(config);
            BaseItem.ConfigurationManager = mockManager.Object;
        }

        [Theory]
        [InlineData("FC2PPV-3068336", "FC2-3068336", PornMovieFlags.Uncensored, AVCategory.Amateur)]
        [InlineData("FC2-PPV-3068336", "FC2-3068336", PornMovieFlags.Uncensored, AVCategory.Amateur)]
        [InlineData("FC2-3068336", "FC2-3068336", PornMovieFlags.Uncensored, AVCategory.Amateur)]
        [InlineData("FC2-3068336 sdsdsdsds", "FC2-3068336", PornMovieFlags.Uncensored, AVCategory.Amateur)]
        [InlineData("sdsdsdsds FC2-3068336", "FC2-3068336", PornMovieFlags.Uncensored, AVCategory.Amateur)]
        [InlineData("FC2-3068336-C", "FC2-3068336", PornMovieFlags.Uncensored | PornMovieFlags.ChineseSubtilte, AVCategory.Amateur)]
        public void TestValidCases(string name, string expectedId, PornMovieFlags flags, AVCategory movieIdCategory)
        {
            var path = name + ".mp4";
            var movie = new PornMovie { Name = name, Path = path };
            var info = movie.PornMovieInfo;
            Assert.NotNull(info);
            Assert.True(info.Valid);
            Assert.Equal(expectedId, info.Id);
            Assert.Equal(flags, info.Flags);
            Assert.Equal(movieIdCategory, info.Category);
            Assert.Equal(path, info.FileName);
        }

        [Fact]
        public void TestValidCasesFromFile()
        {
            var json = File.ReadAllText(Path.Combine("files", "testconfig.json"));
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
                var movie = new PornMovie { Name = name, Path = name + ".mp4" };
                var info = movie.PornMovieInfo;
                Assert.NotNull(info);
                Assert.Equal(expectedId, info.Id);
            }
        }
    }
}
