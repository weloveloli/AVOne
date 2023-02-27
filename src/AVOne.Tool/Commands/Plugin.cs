// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Tool.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Plugins;
    using AVOne.Configuration;
    using AVOne.Tool.Resources;
    using AVOne.Updates;
    using CommandLine;
    using CommandLine.Text;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;

    [Verb("plugin", false, HelpText = nameof(Resource.HelpTextVerbPlugin), ResourceType = typeof(Resource))]
    internal class Plugin : BaseHostOptions
    {
        private IPluginManager pluginManager;
        private IConfigurationManager _configurationManager;
        private IInstallationManager installationManager;
        private ILogger<Plugin> _logger;

        [Option('l', "list", Required = false, Group = "action", HelpText = nameof(Resource.HelpTextShowInstalledPlugins), ResourceType = typeof(Resource))]
        public bool List { get; set; }

        [Option('i', "ins", Required = false, Group = "action", HelpText = nameof(Resource.HelpTextInstallPlugin), ResourceType = typeof(Resource))]
        public string AddPluginOption { get; set; }

        [Usage(ApplicationAlias = ToolAlias)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(nameof(Resource.HelpTextInstallPlugin), new Plugin { AddPluginOption = "MetaTube@2023.227.735.0" });
            }
        }

        public override Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                this.pluginManager = host.Resolve<IPluginManager>();
                this._configurationManager = host.Resolve<IConfigurationManager>();
                this.installationManager = host.Resolve<IInstallationManager>();
                this._logger = host.Resolve<ILogger<Plugin>>();
                if (List)
                {
                    ListPlugins();
                }
                else if (!string.IsNullOrEmpty(AddPluginOption))
                {
                    var values = AddPluginOption.Split('@');
                    var name = values[0];
                    var version = values[1];
                    await InstallPlugin(name, version, token);
                }
            }, token);
        }

        private void ListPlugins()
        {
            var plugins = pluginManager.Plugins;
            Cli.PrintTableEnum(plugins, true,
                Cli.TextDef<LocalPlugin>(nameof(LocalPlugin.Id)),
                Cli.TextDef<LocalPlugin>(nameof(LocalPlugin.Name)),
                Cli.TextDef<LocalPlugin>(nameof(LocalPlugin.Version)),
                Cli.TextDef<LocalPlugin>(nameof(LocalPlugin.Path)),
                Cli.TextDef<LocalPlugin>(nameof(LocalPlugin.IsEnabledAndSupported)));
        }

        private async Task InstallPlugin(string name, string version, CancellationToken cancellationToken)
        {

            // Asynchronous
            await AnsiConsole.Status()
                .StartAsync(Resource.InfoSearchingPlugins, async ctx =>
                {
                    var packages = await installationManager?.GetAvailablePackages(cancellationToken);
                    var packagesToInstall = installationManager?.GetCompatibleVersions(packages, name: name);
                    if (version != "latest")
                    {
                        packagesToInstall = installationManager?.GetCompatibleVersions(packages, name: name, specificVersion: Version.Parse(version));
                    }
                    var package = packagesToInstall.FirstOrDefault();
                    if (package is null)
                    {
                        Cli.Error(Resource.ErrorCannotInstallPlugins, AddPluginOption);
                        return;
                    }

                    try
                    {
                        ctx.Status(string.Format(Resource.InfoDownloadingPlugin, AddPluginOption));
                        await installationManager.InstallPackage(package, cancellationToken).ConfigureAwait(false);
                        ctx.Status(string.Format(Resource.SuccessDownloadPlugin, AddPluginOption));
                        Cli.Success(Resource.SuccessDownloadPlugin, AddPluginOption);
                    }
                    catch (OperationCanceledException)
                    {
                        // InstallPackage has it's own inner cancellation token, so only throw this if it's ours
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw;
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError(ex, Resource.ErrorCannotInstallPlugins, package.Name);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, Resource.ErrorCannotInstallPlugins, package.Name);
                    }
                    catch (InvalidDataException ex)
                    {
                        _logger.LogError(ex, Resource.ErrorCannotInstallPlugins, package.Name);
                    }

                });
        }
    }
}
