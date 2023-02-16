// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

using System.Reflection;
using AVOne.Tool;
using AVOne.Tool.Commands;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

// Set sentence builder to localizable
SentenceBuilder.Factory = () => new LocalizableSentenceBuilder();
var optionTypes = typeof(Program).Assembly.GetTypes()
        .Where(t => t.IsSubclassOf(typeof(BaseOptions)))
        .ToArray();
if (Parser.Default.ParseArguments(args, optionTypes) is Parsed<object> parsed)
{
    if (parsed.Value is BaseOptions option)
    {
        AppDomain.CurrentDomain.AssemblyResolve += StartupHelpers.ResolveJellyfinAssemlyRenameHandler;
        var appPaths = StartupHelpers.CreateApplicationPaths(option);
        var appHost = await StartupHelpers.CreateConsoleAppHost(option, appPaths);
        var options = GenericRunOptions.Default
            .WithArgs(args)
            .Silence(true, false)
            .ConfigureBuilder((builder) =>
            {
                builder.ConfigureLogging((logging) =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog(Log.Logger);
                });
                return builder.UseContentRoot(StartupHelpers.RealRootContentPath);
            })
            .ConfigureServices(appHost.Init);
        var host = Serve.Run(options);
        try
        {
            // Re-use the host service provider in the app host since ASP.NET doesn't allow a custom service collection.
            appHost.ServiceProvider = host.Services;
            await appHost.InitializeServices().ConfigureAwait(false);
            await option.ExecuteAsync(appHost, StartupHelpers.TokenSource.Token).ConfigureAwait(false);
            return 0;
        }
        catch (Exception e)
        {
            var type = option.GetType();
            var cmdName = type.GetCustomAttribute<VerbAttribute>()?.Name ?? type.Name;
            StartupHelpers.Logger.LogError(e, "Command {0} execute error", cmdName);
            Cli.Exception(e, $"Command {cmdName} execute error");
            Environment.Exit(1);
        }
    }
}
return 1;
