// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

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
