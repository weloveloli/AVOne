// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Tool.Resources;
    using CommandLine;
    using MediaBrowser.Common.Configuration;

    [Verb("info", false, HelpText = "HelpTextVerbInfo", ResourceType = typeof(Resource))]
    internal class Info : BaseHostOptions
    {
        /// <summary>
        /// Gets or sets the path to the data directory.
        /// </summary>
        /// <value>The path to the data directory.</value>
        [Option("env", Required = false, Group = "type", Default = true, HelpText = "Print environment information.")]
        public bool Env { get; set; }

        internal override Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            return Task.Run(() => Execute(host));
        }
        private void Execute(ConsoleAppHost host)
        {
            var appPaths = host.Resolve<IApplicationPaths>();
            if (Env)
            {
                PrintEnvironmentInfo(appPaths);
            }
        }

        /// <summary>
        /// Logs relevant environment variables and information about the host.
        /// </summary>
        /// <param name="appPaths">The application paths to use.</param>
        internal static void PrintEnvironmentInfo(IApplicationPaths appPaths)
        {
            // Distinct these to prevent users from reporting problems that aren't actually problems
            var commandLineArgs = Environment
                .GetCommandLineArgs()
                .Distinct();

            // Get all relevant environment variables
            var allEnvVars = Environment.GetEnvironmentVariables();
            var relevantEnvVars = new Dictionary<object, object>();
            foreach (var key in allEnvVars.Keys)
            {
                if (StartupHelpers.RelevantEnvVarPrefixes.Any(prefix => key.ToString()!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    relevantEnvVars.Add(key, allEnvVars[key]!);
                }
            }

            Cli.Print("Environment Variables: ", ToString(relevantEnvVars.Select(e => $"[{e.Key}:{e.Value}]")));
            Cli.Print("Arguments: ", ToString(commandLineArgs));
            Cli.Print("Operating system: ", RuntimeInformation.OSDescription);
            Cli.Print("Architecture: ", RuntimeInformation.OSArchitecture);
            Cli.Print("64-Bit Process: ", Environment.Is64BitProcess);
            Cli.Print("User Interactive: ", Environment.UserInteractive);
            Cli.Print("Processor count: ", Environment.ProcessorCount);
            Cli.Print("Program data path: ", appPaths.ProgramDataPath);
            Cli.Print("Web resources path: ", appPaths.WebPath);
            Cli.Print("Application directory: ", appPaths.ProgramSystemPath);
        }
        static string ToString(IEnumerable<object> values)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var value in values)
            {
                sb.Append(value);
                sb.Append(',');
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
