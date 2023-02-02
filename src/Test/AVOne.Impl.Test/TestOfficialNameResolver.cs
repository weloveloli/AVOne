// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Test
{
    using AVOne.Impl.Providers.Official;

    public class TestOfficialNameResolver
    {
        [Theory]
        [InlineData("abc_88", "abc-88")]
        [InlineData("abc-88-C", "abc-88")]
        [InlineData("FC2PPV-3068336", "FC2-3068336")]
        public void TestNameResolver(string name, string expectedId)
        {
            var provider = new OfficialMovieNameParserProvider();

            var id = provider.Parse(name);

            Assert.Equal(expectedId, id.Id);
        }
    }
}
