// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using AVOne.Constants;
    using AVOne.Tool.Commands;
    using AVOne.Tool.Configuration;
    using AVOne.Tool.Migrations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Serilog;
    using Serilog.Extensions.Logging;
    using ILogger = Microsoft.Extensions.Logging.ILogger;
    using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

    /// <summary>
    /// A class containing helper methods for server startup.
    /// </summary>
    internal static class StartupHelpers
    {
        internal static readonly ILoggerFactory LoggerFactory = new SerilogLoggerFactory();
        internal static ILogger Logger = NullLogger.Instance;
        internal static CancellationTokenSource TokenSource = new();
        /// <summary>
        /// The name of logging configuration file containing application defaults.
        /// </summary>
        internal const string LoggingConfigFileDefault = "logging.default.json";

        /// <summary>
        /// The name of the logging configuration file containing the system-specific override settings.
        /// </summary>
        internal const string LoggingConfigFileSystem = "logging.json";

        internal const string AVOnePrefix = "AVONE_TOOL_";
        internal const string AVONE_TOOL_NAME = "avonetool";
        internal static readonly string[] RelevantEnvVarPrefixes = { AVOnePrefix, "DOTNET_", "ASPNETCORE_" };
        internal static string RealRootContentPath => Directory.GetParent(typeof(StartupHelpers).Assembly.Location)?.FullName ?? Directory.GetCurrentDirectory();
        /// <summary>
        /// Create the data, config and log paths from the variety of inputs(command line args,
        /// environment variables) or decide on what default to use. For Windows it's %AppPath%
        /// for everything else the
        /// <a href="https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html">XDG approach</a>
        /// is followed.
        /// </summary>
        /// <param name="options">The <see cref="StartupOptions" /> for this instance.</param>
        /// <returns><see cref="ServerApplicationPaths" />.</returns>
        internal static ConsoleApplicationPaths CreateApplicationPaths(BaseHostOptions options)
        {
            // LocalApplicationData
            // Windows: %LocalAppData%
            // macOS: NSApplicationSupportDirectory
            // UNIX: $XDG_DATA_HOME
            var dataDir = options.DataDir
                ?? Environment.GetEnvironmentVariable(AVOnePrefix + "DATA_DIR")
                ?? Path.Join(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    AVONE_TOOL_NAME);

            var configDir = Environment.GetEnvironmentVariable(AVOnePrefix + "CONFIG_DIR");
            if (configDir is null)
            {
                configDir = Path.Join(dataDir, "config");
                if (options.DataDir is null
                    && !Directory.Exists(configDir)
                    && !OperatingSystem.IsWindows()
                    && !OperatingSystem.IsMacOS())
                {
                    // UNIX: $XDG_CONFIG_HOME
                    configDir = Path.Join(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "avone");
                }
            }

            var cacheDir = Environment.GetEnvironmentVariable(AVOnePrefix + "CACHE_DIR");
            if (cacheDir is null)
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
                {
                    cacheDir = Path.Join(dataDir, "cache");
                }
                else
                {
                    cacheDir = Path.Join(GetXdgCacheHome(), "avone");
                }
            }

            var logDir = Environment.GetEnvironmentVariable(AVOnePrefix + "LOG_DIR");
            logDir ??= Path.Join(dataDir, "log");

            // Normalize paths. Only possible with GetFullPath for now - https://github.com/dotnet/runtime/issues/2162
            dataDir = Path.GetFullPath(dataDir);
            logDir = Path.GetFullPath(logDir);
            configDir = Path.GetFullPath(configDir);
            cacheDir = Path.GetFullPath(cacheDir);

            // Ensure the main folders exist before we continue
            try
            {
                _ = Directory.CreateDirectory(dataDir);
                _ = Directory.CreateDirectory(logDir);
                _ = Directory.CreateDirectory(configDir);
                _ = Directory.CreateDirectory(cacheDir);
            }
            catch (IOException ex)
            {
                Cli.Exception(ex, "Error whilst attempting to create folder");
                Environment.Exit(1);
            }

            return new ConsoleApplicationPaths(dataDir, logDir, configDir, cacheDir);
        }

        internal static IHost CreateHost(string[] args, ConsoleAppHost appHost)
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
                        _ = logging.AddSerilog(Log.Logger);
                    });

                    //return builder.UseContentRoot(StartupHelpers.RealRootContentPath);
                    return builder;
                })
                .ConfigureServices(appHost.Init);
            var host = Serve.Run(options);
            // Re-use the host service provider in the app host since ASP.NET doesn't allow a custom service collection.
            appHost.ServiceProvider = host.Services;
            appHost.PostBuildService();
            MigrationRunner.Run(appHost, LoggerFactory);
            return host;
        }

        private static string GetXdgCacheHome()
        {
            // $XDG_CACHE_HOME defines the base directory relative to which
            // user specific non-essential data files should be stored.
            var cacheHome = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");

            // If $XDG_CACHE_HOME is either not set or a relative path,
            // a default equal to $HOME/.cache should be used.
            if (cacheHome is null || !cacheHome.StartsWith('/'))
            {
                cacheHome = Path.Join(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".cache");
            }

            return cacheHome;
        }

        internal static IConfiguration CreateAppConfiguration(BaseHostOptions commandLineOpts, ConsoleApplicationPaths appPaths)
        {
            return new ConfigurationBuilder()
                    .SetBasePath(appPaths.ConfigurationDirectoryPath)
                    .AddJsonFile(LoggingConfigFileDefault, optional: false, reloadOnChange: true)
                    .AddJsonFile(LoggingConfigFileSystem, optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(AVOnePrefix)
                    .AddInMemoryCollection(commandLineOpts.ConvertToConfig())
                    .Build();
        }

        /// <summary>
        /// Initialize the logging configuration file using the bundled resource file as a default if it doesn't exist
        /// already.
        /// </summary>
        /// <param name="appPaths">The application paths.</param>
        /// <returns>A task representing the creation of the configuration file, or a completed task if the file already exists.</returns>
        internal static async Task InitLoggingConfigFile(ConsoleApplicationPaths appPaths)
        {
            // Do nothing if the config file already exists
            var configPath = Path.Combine(appPaths.ConfigurationDirectoryPath, LoggingConfigFileDefault);
            if (File.Exists(configPath))
            {
                return;
            }

            // Get a stream of the resource contents
            // NOTE: The .csproj name is used instead of the assembly name in the resource path
            const string ResourcePath = "AVOne.Tool.Resources.Configuration.logging.json";
            var resource = typeof(Program).Assembly.GetManifestResourceStream(ResourcePath)
                              ?? throw new InvalidOperationException($"Invalid resource path: '{ResourcePath}'");
            await using (resource.ConfigureAwait(false))
            {
                Stream dst = new FileStream(configPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, AVOneConstants.FileStreamBufferSize, FileOptions.Asynchronous);
                await using (dst.ConfigureAwait(false))
                {
                    // Copy the resource contents to the expected file path for the config file
                    await resource.CopyToAsync(dst).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Initialize Serilog using configuration and fall back to defaults on failure.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="appPaths">The application paths.</param>
        internal static void InitializeLoggingFramework(IConfiguration configuration, ConsoleApplicationPaths appPaths)
        {
            try
            {
                // Serilog.Log is used by SerilogLoggerFactory when no logger is specified
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithThreadId()
                    .CreateLogger();
            }
            catch (Exception ex)
            {
                Log.Logger = new LoggerConfiguration()
                    // .WriteTo.Console(
                    //    outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                    //    formatProvider: CultureInfo.InvariantCulture)
                    .WriteTo.Async(x => x.File(
                        Path.Combine(appPaths.LogDirectoryPath, "log_.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{ThreadId}] {SourceContext}: {Message}{NewLine}{Exception}",
                        formatProvider: CultureInfo.InvariantCulture,
                        encoding: Encoding.UTF8))
                    .Enrich.FromLogContext()
                    .Enrich.WithThreadId()
                    .CreateLogger();

                Log.Logger.Fatal(ex, "Failed to create/read logger configuration");
            }
        }

        internal static async Task<ConsoleAppHost> CreateConsoleAppHost(BaseHostOptions option, ConsoleApplicationPaths appPaths)
        {
            // Log all uncaught exceptions to std error
            static void UnhandledExceptionToConsole(object sender, UnhandledExceptionEventArgs e)
            {
                Cli.Error("Unhandled Exception\n" + e.ExceptionObject.ToString());
            }

            // $AVONETOOL_LOG_DIR needs to be set for the logger configuration manager
            Environment.SetEnvironmentVariable("AVONETOOL_LOG_DIR", appPaths.LogDirectoryPath);
            await InitLoggingConfigFile(appPaths).ConfigureAwait(false);
            // Create an instance of the application configuration to use for application startup
            var startupConfig = CreateAppConfiguration(option, appPaths);
            // Initialize logging framework
            InitializeLoggingFramework(startupConfig, appPaths);
            Logger = LoggerFactory.CreateLogger("Main");
            Cli.Logger = Logger;
            // Log uncaught exceptions to the logging instead of std error
            AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionToConsole;
            AppDomain.CurrentDomain.UnhandledException += (_, e)
                => Logger.LogCritical((Exception)e.ExceptionObject, "Unhandled Exception");

            // Intercept Ctrl+C and Ctrl+Break
            Console.CancelKeyPress += (_, e) =>
            {
                if (TokenSource.IsCancellationRequested)
                {
                    return; // Already shutting down
                }

                e.Cancel = true;
                Logger.LogInformation("Ctrl+C, shutting down");
                Environment.ExitCode = 128 + 2;
                Shutdown();
            };

            // Register a SIGTERM handler
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (TokenSource.IsCancellationRequested)
                {
                    return; // Already shutting down
                }

                Logger.LogInformation("Received a SIGTERM signal, shutting down");
                Environment.ExitCode = 128 + 15;
                Shutdown();
            };
            MigrationRunner.RunPreStartup(appPaths, LoggerFactory);
            return new ConsoleAppHost(option, LoggerFactory, TokenSource, appPaths);
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        internal static void Shutdown()
        {
            if (!TokenSource.IsCancellationRequested)
            {
                TokenSource.Cancel();
            }
        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        internal static void Restart()
        {
            //_restartOnShutdown = true;

            Shutdown();
        }
    }
}
