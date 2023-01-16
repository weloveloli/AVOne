// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Models.Info;
    using AVOne.Tool.Resources;
    using CommandLine;

    [Verb("organize", false, new string[] { "org" }, HelpText = "HelpTextVerbOrganize", ResourceType = typeof(Resource))]
    internal class Organize : BaseOptions
    {
        [Value(0, Required = true, HelpText = "HelpTextDirectory", ResourceType = typeof(Resource))]
        public string? Dir { get; set; }

        public override Task<int> ExecuteAsync(IServiceProvider provider, CancellationToken token)
        {
            if (Directory.Exists(this.Dir))
            {
                Console.Error.WriteLine(Resource.ErrorDirectoryNotExists,Dir);
                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }

        public void ResolveMovies(LinkedList<VideoFileInfo> videoFileInfos, string path)
        {

        }
    }
}
