// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

using Furion.Xunit;
using Xunit;
using Xunit.Abstractions;

[assembly: TestFramework("AVOne.Providers.Official.Tests.TestProgram", "AVOne.Providers.Official.Tests")]
namespace AVOne.Providers.Official.Tests
{
    internal class TestProgram : TestStartup
    {
        public TestProgram(IMessageSink messageSink) : base(messageSink)
        {
            // 初始化 Furion
            Serve.Run(silence: true);
        }
    }
}
