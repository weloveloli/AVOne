// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Tool.Resources;
    using CacheManager.Core.Logging;
    using CommandLine;
    using CommandLine.Text;
    using MediaBrowser.Common.Plugins;
    using MediaBrowser.Common.Updates;
    using MediaBrowser.Controller.Configuration;
    using Microsoft.Extensions.Logging;
    using Spectre.Console;

    [Verb("plugin", false, HelpText = "HelpTextVerbPlugin", ResourceType = typeof(Resource))]
    internal class Plugin : BaseHostOptions
    {
        private IPluginManager pluginManager;
        private IServerConfigurationManager configurationManager;
        private IInstallationManager installationManager;
        private ILogger<Plugin> _logger;

        [Option('l', "list", Required = false, Group = "action", HelpText = "显示插件列表")]
        public bool List { get; set; }

        [Option('a', "add", Required = false, Group = "action", HelpText = "安装插件")]
        public string AddPluginOption { get; set; }

        [Usage(ApplicationAlias = ToolAlias)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("安装插件", new Plugin { AddPluginOption = "MetaTube@2022.1218.1400.0" });
            }
        }

        internal override Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                this.pluginManager = host.Resolve<IPluginManager>();
                this.configurationManager = host.Resolve<IServerConfigurationManager>();
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
            Cli.PrintTable(plugins, true,
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
                .StartAsync("搜索插件...", async ctx =>
                {
                    var packages = await installationManager.GetAvailablePackages(cancellationToken);
                    var packagesToInstall = installationManager.GetCompatibleVersions(packages, name: name);
                    if (version != "latest")
                    {
                        packagesToInstall = installationManager.GetCompatibleVersions(packages, name: name, specificVersion: Version.Parse(version));
                    }
                    var package = packagesToInstall.FirstOrDefault();
                    if (package is null)
                    {
                        Cli.Error("无法安装插件 {0}", AddPluginOption);
                        return;
                    }

                    try
                    {
                        ctx.Status(string.Format("下载安装插件 '{0}'",AddPluginOption));
                        await installationManager.InstallPackage(package, cancellationToken).ConfigureAwait(false);
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
                        _logger.LogError(ex, "Error downloading {0}", package.Name);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "Error updating {0}", package.Name);
                    }
                    catch (InvalidDataException ex)
                    {
                        _logger.LogError(ex, "Error updating {0}", package.Name);
                    }
                    ctx.Status(string.Format("安装插件成功 '{0}'", AddPluginOption));
                    Cli.Success("安装插件 {0} 成功", AddPluginOption);
                });
        }
    }
}
