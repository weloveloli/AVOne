// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the GPLv2 License.

//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;

//using IHost host = Host.CreateDefaultBuilder(args)
//    .ConfigureHostConfiguration(configHost =>
//    {
//        configHost.SetBasePath(Directory.GetCurrentDirectory());
//        configHost.AddEnvironmentVariables(prefix: "AVONE_");
//        configHost.AddCommandLine(args);
//    })
//    .Build();

//// Application code should start here.

//await host.RunAsync();

using System.Reflection;
using System.Text;
using AVOne.Tool;
using AVOne.Tool.Commands;
using AVOne.Tool.Configuration;
using CacheManager.Core.Logging;
using CommandLine;
using Jellyfin.Server.Helpers;
using MediaBrowser.Controller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

class Program
{
    /// <summary>
    /// The name of logging configuration file containing application defaults.
    /// </summary>
    public const string LoggingConfigFileDefault = "logging.default.json";

    /// <summary>
    /// The name of the logging configuration file containing the system-specific override settings.
    /// </summary>
    public const string LoggingConfigFileSystem = "logging.json";
    private static bool _restartOnShutdown;

    private static readonly ILoggerFactory _loggerFactory = new SerilogLoggerFactory();
    private static ILogger _logger = NullLogger.Instance;
    private static CancellationTokenSource _tokenSource = new();
    static async Task<int> Main(string[] args)
    {
        var optionTypes = typeof(Program).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BaseOptions)))
                .ToArray();
        if (Parser.Default.ParseArguments(args, optionTypes) is Parsed<object> parsed)
        {

            if (parsed.Value is BaseOptions option)
            {
                AppDomain.CurrentDomain.AssemblyResolve += StartupHelpers.ResolveJellyfinAssemlyRenameHandler;

                var appPaths = StartupHelpers.CreateApplicationPaths(option);
                var appHost = await CreateHost(option, appPaths);
                var host = Serve.RunGeneric(true, false, args, (service) =>
                {
                    appHost.Init(service);
                });
                try
                {
                    // Re-use the host service provider in the app host since ASP.NET doesn't allow a custom service collection.
                    appHost.ServiceProvider = host.Services;
                    await appHost.InitializeServices().ConfigureAwait(false);
                    await option.ExecuteAsync(appHost, _tokenSource.Token).ConfigureAwait(false);
                    return 0;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "excute error");
                    Console.Error.WriteLine($"Unhandled Exception: {e.Message}\n" + e.StackTrace);
                }
            }
        }
        return 1;
    }

    private static async Task<ConsoleAppHost> CreateHost(BaseOptions option, IServerApplicationPaths appPaths)
    {
        // Log all uncaught exceptions to std error
        static void UnhandledExceptionToConsole(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine("Unhandled Exception\n" + e.ExceptionObject.ToString());
        }

        // $AVONETOOL_LOG_DIR needs to be set for the logger configuration manager
        Environment.SetEnvironmentVariable("AVONETOOL_LOG_DIR", appPaths.LogDirectoryPath);
        await StartupHelpers.InitLoggingConfigFile(appPaths).ConfigureAwait(false);
        // Create an instance of the application configuration to use for application startup
        var startupConfig = CreateAppConfiguration(option, appPaths);
        // Initialize logging framework
        StartupHelpers.InitializeLoggingFramework(startupConfig, appPaths);
        _logger = _loggerFactory.CreateLogger("Main");
        // Log uncaught exceptions to the logging instead of std error
        AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionToConsole;
        AppDomain.CurrentDomain.UnhandledException += (_, e)
            => _logger.LogCritical((Exception)e.ExceptionObject, "Unhandled Exception");

        // Intercept Ctrl+C and Ctrl+Break
        Console.CancelKeyPress += (_, e) =>
        {
            if (_tokenSource.IsCancellationRequested)
            {
                return; // Already shutting down
            }

            e.Cancel = true;
            _logger.LogInformation("Ctrl+C, shutting down");
            Environment.ExitCode = 128 + 2;
            Shutdown();
        };

        // Register a SIGTERM handler
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            if (_tokenSource.IsCancellationRequested)
            {
                return; // Already shutting down
            }

            _logger.LogInformation("Received a SIGTERM signal, shutting down");
            Environment.ExitCode = 128 + 15;
            Shutdown();
        };
        return new ConsoleAppHost(appPaths, _loggerFactory, option, startupConfig);
    }

    private static IConfiguration CreateAppConfiguration(BaseOptions commandLineOpts, IServerApplicationPaths appPaths)
    {
        return new ConfigurationBuilder()
                .SetBasePath(appPaths.ConfigurationDirectoryPath)
                .AddInMemoryCollection(ConfigurationOptions.DefaultConfiguration)
                .AddJsonFile(LoggingConfigFileDefault, optional: false, reloadOnChange: true)
                .AddJsonFile(LoggingConfigFileSystem, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("AVONETOOL_")
                .Build();
    }



    /// <summary>
    /// Shuts down the application.
    /// </summary>
    internal static void Shutdown()
    {
        if (!_tokenSource.IsCancellationRequested)
        {
            _tokenSource.Cancel();
        }
    }

    /// <summary>
    /// Restarts the application.
    /// </summary>
    internal static void Restart()
    {
        _restartOnShutdown = true;

        Shutdown();
    }
}
