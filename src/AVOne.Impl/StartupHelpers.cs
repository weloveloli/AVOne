// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.Impl.Configuration;
    using AVOne.Impl.Migrations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Serilog;
    using Serilog.Extensions.Logging;
    using ILogger = Microsoft.Extensions.Logging.ILogger;
    using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

    /// <summary>
    /// A class containing helper methods for server startup.
    /// </summary>
    public static class StartupHelpers
    {
        public static readonly ILoggerFactory LoggerFactory = new SerilogLoggerFactory();
        public static ILogger Logger = NullLogger.Instance;
        public static CancellationTokenSource TokenSource = new();
        /// <summary>
        /// The name of logging configuration file containing application defaults.
        /// </summary>
        internal static string LoggingConfigFileDefault => (IsTool() ? "logging.console.default.json" : "logging.default.json");

        /// <summary>
        /// The name of the logging configuration file containing the system-specific override settings.
        /// </summary>
        internal const string LoggingConfigFileSystem = "logging.json";

        public const string AVOnePrefix = "AVONE";
        internal const string AVONE_NAME = "avone";
        internal static readonly string[] RelevantEnvVarPrefixes = { AVOnePrefix, "DOTNET_", "ASPNETCORE_" };
        internal static string RealRootContentPath => AppContext.BaseDirectory;
        /// <summary>
        /// Create the data, config and log paths from the variety of inputs(command line args,
        /// environment variables) or decide on what default to use. For Windows it's %AppPath%
        /// for everything else the
        /// <a href="https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html">XDG approach</a>
        /// is followed.
        /// </summary>
        /// <param name="options">The <see cref="StartupOptions" /> for this instance.</param>
        /// <returns><see cref="ServerApplicationPaths" />.</returns>
        public static ApplicationPaths CreateApplicationPaths(IStartupOptions options)
        {
            // LocalApplicationData
            // Windows: %LocalAppData%
            // macOS: NSApplicationSupportDirectory
            // UNIX: $XDG_DATA_HOME
            var dataDir = options.DataDir
                ?? Environment.GetEnvironmentVariable(AVOnePrefix + "DATA_DIR")
                ?? Path.Join(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    AVONE_NAME);

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
                        AVONE_NAME);
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
                    cacheDir = Path.Join(GetXdgCacheHome(), AVONE_NAME);
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
                Logger.LogError(ex, "Error whilst attempting to create folder");
                Environment.Exit(1);
            }

            return new ApplicationPaths(dataDir, logDir, configDir, cacheDir);
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

        public static IConfiguration CreateAppConfiguration(IStartupOptions commandLineOpts, ApplicationPaths appPaths)
        {
            return new ConfigurationBuilder()
                    .SetBasePath(appPaths.ConfigurationDirectoryPath)
                    .AddJsonFile(LoggingConfigFileDefault, optional: false, reloadOnChange: true)
                    .AddJsonFile(LoggingConfigFileSystem, optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(AVOnePrefix)
                    .Build();
        }

        /// <summary>
        /// Initialize the logging configuration file using the bundled resource file as a default if it doesn't exist
        /// already.
        /// </summary>
        /// <param name="appPaths">The application paths.</param>
        /// <returns>A task representing the creation of the configuration file, or a completed task if the file already exists.</returns>
        public static async Task InitLoggingConfigFile(ApplicationPaths appPaths)
        {
            // Do nothing if the config file already exists
            var configPath = Path.Combine(appPaths.ConfigurationDirectoryPath, LoggingConfigFileDefault);
            if (!File.Exists(configPath) && !IsUseDefaultLogging())
            {
                // Get a stream of the resource contents
                // NOTE: The .csproj name is used instead of the assembly name in the resource path
                var ResourcePath = IsTool() ? "AVOne.Impl.Configuration.logging.console.json" : "AVOne.Impl.Configuration.logging.json";
                var resource = typeof(StartupHelpers).Assembly.GetManifestResourceStream(ResourcePath)
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
        }
        public static bool IsTool()
        {
            return bool.Parse(Environment.GetEnvironmentVariable($"{AVOnePrefix}_TOOL") ?? "false");
        }

        public static bool IsUseDefaultLogging()
        {
            return bool.Parse(Environment.GetEnvironmentVariable($"{AVOnePrefix}_USE_DEFAULT_LOG") ?? "false");
        }
        /// <summary>
        /// Initialize Serilog using configuration and fall back to defaults on failure.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="appPaths">The application paths.</param>
        public static void InitializeLoggingFramework(IConfiguration configuration, ApplicationPaths appPaths)
        {
            try
            {
                if (!IsUseDefaultLogging())
                {
                    // Serilog.Log is used by SerilogLoggerFactory when no logger is specified
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithThreadId()
                        .CreateLogger();
                }
            }
            catch (Exception ex)
            {
                Log.Logger = new LoggerConfiguration()
                     .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                        formatProvider: CultureInfo.InvariantCulture)
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

        public static async Task<ApplicationAppHost> CreateConsoleAppHost(IStartupOptions option, ApplicationPaths appPaths)
        {
            // Log all uncaught exceptions to std error
            static void UnhandledExceptionToConsole(object sender, UnhandledExceptionEventArgs e)
            {
                Console.Error.WriteLine("Unhandled Exception\n" + e.ExceptionObject.ToString());
            }

            // $AVONETOOL_LOG_DIR needs to be set for the logger configuration manager
            Environment.SetEnvironmentVariable("AVONETOOL_LOG_DIR", appPaths.LogDirectoryPath);
            await InitLoggingConfigFile(appPaths).ConfigureAwait(false);
            // Create an instance of the application configuration to use for application startup
            var startupConfig = CreateAppConfiguration(option, appPaths);
            // Initialize logging framework
            InitializeLoggingFramework(startupConfig, appPaths);
            Logger = LoggerFactory.CreateLogger("Main");
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
            return new ApplicationAppHost(option, LoggerFactory, TokenSource, appPaths);
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public static void Shutdown()
        {
            if (!TokenSource.IsCancellationRequested)
            {
                TokenSource.Cancel();
            }
        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        public static void Restart()
        {
            //_restartOnShutdown = true;

            Shutdown();
        }
    }
}
