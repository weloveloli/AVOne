// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Test.Base
{
    using AutoFixture;
    using AutoFixture.AutoMoq;

    public abstract class BaseTestCase
    {
        protected readonly IFixture fixture;

        public BaseTestCase()
        {
            fixture = new Fixture().Customize(new AutoMoqCustomization());
        }
    }
}
