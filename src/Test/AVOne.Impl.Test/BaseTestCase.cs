// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Test
{
    using AutoFixture;
    using AutoFixture.AutoMoq;

    public abstract class BaseTestCase
    {
        protected readonly IFixture _fixture;

        public BaseTestCase()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
        }
    }
}
