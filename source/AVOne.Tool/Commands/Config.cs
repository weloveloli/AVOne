// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Tool.Resources;
    using CommandLine;

    [Verb("config", false, new string[] { "config" }, HelpText = "HelpTextVerbConfig", ResourceType = typeof(Resource))]
    internal class Config : BaseOptions
    {
        [Option("get", Group = "Action", Required = false, HelpText = "get value: name [value-pattern]")]
        public string? GetKey { get; set; }

        [Option("get-all", Group = "Action", Required = false, HelpText = "get all values: key [value-pattern]")]
        public bool GetAllKey { get; set; }

        [Option("set", Group = "Action", Required = false, HelpText = "set key value", Max = 2, Min = 2)]
        public IEnumerable<string>? SetKeyValue { get; set; }

        internal override Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
