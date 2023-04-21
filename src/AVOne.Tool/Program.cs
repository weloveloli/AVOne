// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

using System.Reflection;
using AVOne.Impl;
using AVOne.Impl.Migrations;
using AVOne.Tool.Commands;
using AVOne.Tool.Resources;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AVOne.Tool
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // Set sentence builder to localizable
            SentenceBuilder.Factory = () => new LocalizableSentenceBuilder();
            var types = LoadVerbs();
            if (Parser.Default.ParseArguments(args, types) is Parsed<object> parsed)
            {
                if (parsed.Value is BaseHostOptions option)
                {
                    Environment.SetEnvironmentVariable($"{StartupHelpers.AVOnePrefix}_USE_DEFAULT_LOG", "false");
                    var appPaths = StartupHelpers.CreateApplicationPaths(option);
                    var appHost = await StartupHelpers.CreateConsoleAppHost(option, appPaths);
                    var host = CreateHost(args, appHost);
                    Cli.Logger = StartupHelpers.Logger;
                    try
                    {
                        await option.ExecuteAsync(appHost, StartupHelpers.TokenSource.Token).ConfigureAwait(false);
                        return 0;
                    }
                    catch (AppFriendlyException e)
                    {
                        var type = option.GetType();
                        var cmdName = type.GetCustomAttribute<VerbAttribute>()?.Name ?? type.Name;

                        Cli.Error(Resource.ErrorCommand, cmdName);
                        Cli.Error(e.ErrorMessage?.ToString() ?? string.Empty);
                        StartupHelpers.Logger.LogError(e, Resource.ErrorCommand, cmdName);
                        Environment.Exit(1);
                    }
                    catch (Exception e)
                    {
                        var type = option.GetType();
                        var cmdName = type.GetCustomAttribute<VerbAttribute>()?.Name ?? type.Name;

                        StartupHelpers.Logger.LogCritical(e, Resource.ErrorCommand, cmdName);
                        Cli.Info("See error logs in {0}", appPaths.LogDirectoryPath);
                        Environment.Exit(1);
                    }
                }
            }
            return 1;
        }

        //load all types using Reflection
        public static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
        }

        public static IHost CreateHost(string[] args, ApplicationAppHost appHost)
        {
            var options =
                GenericRunOptions.Default
                .WithArgs(args)
                .Silence(true, false)
                .ConfigureBuilder((builder) =>
                {
                    _ = builder.ConfigureLogging((logging) =>
                    {
                        _ = logging.ClearProviders();
                        _ = logging.AddSerilog(Serilog.Log.Logger);
                    });

                    //return builder.UseContentRoot(StartupHelpers.RealRootContentPath);
                    return builder;
                })
                .ConfigureServices(appHost.Init);
            var host = Serve.Run(options);
            // Re-use the host service provider in the app host since ASP.NET doesn't allow a custom service collection.
            appHost.ServiceProvider = host.Services;
            appHost.PostBuildService();
            MigrationRunner.Run(appHost, StartupHelpers.LoggerFactory);
            return host;
        }
    }
}
