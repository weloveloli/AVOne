// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Commands
{
    using AVOne.Tool.Resources;
    using CommandLine;

    [Verb("scan", false, new string[] { "scan" }, HelpText = "HelpTextVerbOrganize", ResourceType = typeof(Resource))]
    public class Scan
    {
    }
}
