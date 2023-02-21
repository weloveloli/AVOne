// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Models.Info;
    using AVOne.Tool.Resources;
    using CommandLine;

    [Verb("organize", false, new string[] { "org" }, HelpText = "HelpTextVerbOrganize", ResourceType = typeof(Resource))]
    internal class Organize : BaseHostOptions
    {
        [Value(0, Required = true, HelpText = "HelpTextDirectory", ResourceType = typeof(Resource))]
        public string? Dir { get; set; }

        public override Task<int> ExecuteAsync(ConsoleAppHost provider, CancellationToken token)
        {
            if (Directory.Exists(Dir))
            {
                Console.Error.WriteLine(Resource.ErrorDirectoryNotExists, Dir);
                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }

        public void ResolveMovies(LinkedList<VideoFileInfo> videoFileInfos, string path)
        {

        }
    }
}
