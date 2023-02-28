// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

using System.Reflection;
using AVOne.Tool;
using AVOne.Tool.Commands;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Logging;

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
                var appPaths = StartupHelpers.CreateApplicationPaths(option);
                var appHost = await StartupHelpers.CreateConsoleAppHost(option, appPaths);
                var host = StartupHelpers.CreateHost(args, appHost);
                try
                {
                    await option.ExecuteAsync(appHost, StartupHelpers.TokenSource.Token).ConfigureAwait(false);
                    return 0;
                }
                catch (AppFriendlyException e)
                {
                    var type = option.GetType();
                    var cmdName = type.GetCustomAttribute<VerbAttribute>()?.Name ?? type.Name;

                    Cli.Error("Command {0} execute failed", cmdName);
                    Cli.Error(e.ErrorMessage.ToString());

                    Environment.Exit(1);
                }
                catch (Exception e)
                {
                    var type = option.GetType();
                    var cmdName = type.GetCustomAttribute<VerbAttribute>()?.Name ?? type.Name;
                    StartupHelpers.Logger.LogCritical(e, "Command {0} execute error due to Exception", cmdName);
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
}
