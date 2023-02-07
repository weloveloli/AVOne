// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool
{
    using System.Text;
    using AVOne.Tool.Commands;
    using CommandLine;
    using CommandLine.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Serilog;
    using Serilog.Extensions.Logging;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    internal class Program
    {
        /// <summary>
        /// The name of logging configuration file containing application defaults.
        /// </summary>
        public const string LoggingConfigFileDefault = "logging.default.json";

        /// <summary>
        /// The name of the logging configuration file containing the system-specific override settings.
        /// </summary>
        public const string LoggingConfigFileSystem = "logging.json";

        private static readonly CancellationTokenSource _tokenSource = new();

        private static readonly ILoggerFactory _loggerFactory = new SerilogLoggerFactory();

        private static ILogger _logger = NullLogger.Instance;

        private static async Task<int> Main(string[] args)
        {
            // Set sentence builder to localizable
            SentenceBuilder.Factory = () => new LocalizableSentenceBuilder();
            var optionTypes = typeof(Program).Assembly.GetTypes()
                                      .Where(t => t.IsSubclassOf(typeof(BaseOptions)))
                                      .ToArray();
            if (Parser.Default.ParseArguments(args, optionTypes) is Parsed<object> parsed)
            {
                if (parsed.Value is BaseOptions option)
                {
                    var appPaths = ConsoleApplicationPaths.CreateConsoleApplicationPaths(option);
                    var host = await CreateHost(option, appPaths);
                    return await host.ExecuteCmd().ConfigureAwait(false);
                }
            }

            return 1;
        }

        private static async Task<ConsoleAppHost> CreateHost(BaseOptions option, ConsoleApplicationPaths appPaths)
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
            _logger = _loggerFactory.CreateLogger("Main");
            // Log uncaught exceptions to the logging instead of std error
            AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionToConsole;
            AppDomain.CurrentDomain.UnhandledException += (_, e)
                => _logger.LogCritical((Exception)e.ExceptionObject, "Unhandled Exception");

            var collection = new ServiceCollection();
            collection.TryAdd(ServiceDescriptor.Singleton(_loggerFactory));
            collection.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
            _ = collection.AddHttpClient();
            var serviceProvider = collection.BuildServiceProvider();

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
            return new ConsoleAppHost(option, _loggerFactory, _tokenSource, appPaths);
        }

        private static IConfiguration CreateAppConfiguration(BaseOptions commandLineOpts, ConsoleApplicationPaths appPaths)
        {
            return new ConfigurationBuilder()
                    .SetBasePath(appPaths.ConfigurationDirectoryPath)
                    .AddInMemoryCollection(ConfigurationOptions.DefaultConfiguration)
                    .AddJsonFile(LoggingConfigFileDefault, optional: false, reloadOnChange: true)
                    .AddJsonFile(LoggingConfigFileSystem, optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables("AVONETOOL_")
                    .AddInMemoryCollection(commandLineOpts.ConvertToConfig())
                    .Build();
        }

        private static async Task InitLoggingConfigFile(ConsoleApplicationPaths appPaths)
        {
            // Do nothing if the config file already exists
            var configPath = Path.Combine(appPaths.ConfigurationDirectoryPath, "logging.default.json");
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
                Stream dst = new FileStream(configPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:指定 IFormatProvider", Justification = "<挂起>")]
        private static void InitializeLoggingFramework(IConfiguration configuration, ConsoleApplicationPaths appPaths)
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
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                    .WriteTo.Async(x => x.File(
                        Path.Combine(appPaths.LogDirectoryPath, "log_.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{ThreadId}] {SourceContext}: {Message}{NewLine}{Exception}",
                        encoding: Encoding.UTF8))
                    .Enrich.FromLogContext()
                    .Enrich.WithThreadId()
                    .CreateLogger();

                Log.Logger.Fatal(ex, "Failed to create/read logger configuration");
            }
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
    }
}
