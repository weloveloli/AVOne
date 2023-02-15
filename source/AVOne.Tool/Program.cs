// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the GPLv2 License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configHost =>
    {
        configHost.SetBasePath(Directory.GetCurrentDirectory());
        configHost.AddEnvironmentVariables(prefix: "AVONE_");
        configHost.AddCommandLine(args);
    })
    .Build();

// Application code should start here.

await host.RunAsync();
