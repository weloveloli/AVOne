// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Commands
{
    using System;
    using AVOne.Tool.Resources;
    using CommandLine;
    using CommandLine.Text;
    using Microsoft.Extensions.Logging;

    [Verb("searchmetadata", false, new string[] { "sm" }, HelpText = "HelpTextVerbSearchMetadata", ResourceType = typeof(Resource))]
    internal class SearchMetadata : BaseOptions
    {
        public bool Debug { get; set; }

        public bool Quiet { get; set; }

        [Value(0, Required = true, HelpText = "HelpTextFileName", ResourceType = typeof(Resource))]
        public string? FileName { get; set; }

        [Usage(ApplicationAlias = "AVOneTool")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(Resource.ExamplesNormalScenario, new SearchMetadata { FileName = "MUM-120" });
            }
        }

        public override Task<int> ExecuteAsync(ConsoleAppHost provider, CancellationToken token)
        {
            var client = provider.Resolve<HttpClient>();
            var logger = provider.Resolve<ILogger<SearchMetadata>>();
            logger?.LogDebug($"client null is {client is null}");
            logger?.LogDebug($"logger null is {logger is null}");
            Thread.Sleep(TimeSpan.FromSeconds(30));
            return Task.FromResult(0);
        }
    }
}
