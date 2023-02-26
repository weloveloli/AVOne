// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Configuration
{
    using AVOne.Configuration;
    using AVOne.Tool.Commands;

    public class ConsoleApplicationPaths : IApplicationPaths
    {
        public ConsoleApplicationPaths(
            string programDataPath,
            string logDirectoryPath,
            string configurationDirectoryPath,
            string cacheDirectoryPath)
        {
            ProgramDataPath = programDataPath;
            LogDirectoryPath = logDirectoryPath;
            ConfigurationDirectoryPath = configurationDirectoryPath;
            CachePath = cacheDirectoryPath;
            DataPath = Directory.CreateDirectory(Path.Combine(ProgramDataPath, "data")).FullName;
        }
        /// <summary>
        /// Gets the path to the program data folder.
        /// </summary>
        /// <value>The program data path.</value>
        public string ProgramDataPath { get; }
        public string LogDirectoryPath { get; private set; }
        public string ConfigurationDirectoryPath { get; private set; }
        public string CachePath { get; private set; }

        /// <summary>
        /// Gets the path to the system folder.
        /// </summary>
        /// <value>The path to the system folder.</value>
        public string ProgramSystemPath { get; } = AppContext.BaseDirectory;

        /// <summary>
        /// Gets the folder path to the data directory.
        /// </summary>
        /// <value>The data directory.</value>
        public string DataPath { get; }

        /// <summary>
        /// Gets the image cache path.
        /// </summary>
        /// <value>The image cache path.</value>
        public string ImageCachePath => Path.Combine(CachePath, "images");

        /// <summary>
        /// Gets the path to the plugin directory.
        /// </summary>
        /// <value>The plugins path.</value>
        public string PluginsPath => Path.Combine(ProgramDataPath, "plugins");

        /// <summary>
        /// Gets the path to the plugin configurations directory.
        /// </summary>
        /// <value>The plugin configurations path.</value>
        public string PluginConfigurationsPath => Path.Combine(PluginsPath, "configurations");

        /// <summary>
        /// Gets the folder path to the temp directory within the cache folder.
        /// </summary>
        /// <value>The temp directory.</value>
        public string TempDirectory => Path.Combine(CachePath, "temp");

        /// <summary>
        /// Gets the path to the system configuration file.
        /// </summary>
        /// <value>The system configuration file path.</value>
        public string SystemConfigurationFilePath => Path.Combine(ConfigurationDirectoryPath, "console.xml");

        public string RootFolderPath => Path.Join(ProgramDataPath, "root");

        public string DefaultInternalMetadataPath => Path.Combine(ProgramDataPath, "metadata");

        public string PeoplePath => Path.Combine(DefaultInternalMetadataPath, "People");

        internal static ConsoleApplicationPaths CreateConsoleApplicationPaths(BaseHostOptions options)
        {
            // dataDir
            // IF      --datadir
            // ELSE IF $AVONETOOL_DATA_DIR
            // ELSE IF windows, use <%APPDATA%>/AVOneTool
            // ELSE IF $XDG_DATA_HOME then use $XDG_DATA_HOME/AVOneTool
            // ELSE    use $HOME/.local/share/AVOneTool
            var dataDir = options.DataDir;
            if (string.IsNullOrEmpty(dataDir))
            {
                dataDir = Environment.GetEnvironmentVariable("AVONETOOL_DATA_DIR");

                if (string.IsNullOrEmpty(dataDir))
                {
                    // LocalApplicationData follows the XDG spec on unix machines
                    dataDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "AVOneTool");
                }
            }

            // configDir
            // IF $AVONETOOL_CONFIG_DIR
            // ELSE IF --datadir, use <datadir>/config (assume portable run)
            // ELSE IF <datadir>/config exists, use that
            // ELSE IF windows, use <datadir>/config
            // ELSE    $HOME/.config/AVOneTool
            var configDir = Environment.GetEnvironmentVariable("AVONETOOL_CONFIG_DIR");

            if (string.IsNullOrEmpty(configDir))
            {
                if (options.DataDir != null
                        || Directory.Exists(Path.Combine(dataDir, "config"))
                        || OperatingSystem.IsWindows())
                {
                    // Hang config folder off already set dataDir
                    configDir = Path.Combine(dataDir, "config");
                }
                else
                {
                    if (string.IsNullOrEmpty(configDir))
                    {
                        configDir = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                            ".config");
                    }

                    configDir = Path.Combine(configDir, "AVOneTool");
                }
            }

            // cacheDir
            // IF $AVONETOOL_CACHE_DIR
            // ELSE IF windows, use <datadir>/cache
            // ELSE    HOME/.cache/AVOneTool
            var cacheDir = Environment.GetEnvironmentVariable("AVONETOOL_CACHE_DIR");

            if (string.IsNullOrEmpty(cacheDir))
            {
                if (OperatingSystem.IsWindows())
                {
                    // Hang cache folder off already set dataDir
                    cacheDir = Path.Combine(dataDir, "cache");
                }
                else
                {
                    // $XDG_CACHE_HOME defines the base directory relative to which
                    // user specific non-essential data files should be stored.
                    cacheDir = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");

                    // If $XDG_CACHE_HOME is either not set or empty,
                    // a default equal to $HOME/.cache should be used.
                    if (string.IsNullOrEmpty(cacheDir))
                    {
                        cacheDir = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                            ".cache");
                    }

                    cacheDir = Path.Combine(cacheDir, "AVOneTool");
                }
            }

            // logDir
            // IF      --logdir
            // ELSE IF $AVONETOOL_LOG_DIR
            // ELSE IF --datadir, use <datadir>/log (assume portable run)
            // ELSE    <datadir>/log
            var logDir = Environment.GetEnvironmentVariable("AVONETOOL_LOG_DIR");

            if (string.IsNullOrEmpty(logDir))
            {
                // Hang log folder off already set dataDir
                logDir = Path.Combine(dataDir, "log");
            }

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
                Console.Error.WriteLine("Error whilst attempting to create folder");
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1);
            }

            return new ConsoleApplicationPaths(dataDir, logDir, configDir, cacheDir);
        }
    }
}
