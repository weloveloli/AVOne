// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Tool.Resources;
    using CommandLine;

    [Verb("scan", false, new string[] { "scan" }, HelpText = "HelpTextVerbScan", ResourceType = typeof(Resource))]
    internal class Scan : BaseOptions
    {
        public override Task<int> ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
