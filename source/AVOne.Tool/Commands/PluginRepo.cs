// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System.Threading.Tasks;
    using CommandLine;
    using System.Threading;
    using MediaBrowser.Common.Plugins;
    using MediaBrowser.Common.Updates;
    using MediaBrowser.Controller.Configuration;
    using MediaBrowser.Model.Updates;
    using Spectre.Console.Rendering;
    using CommandLine.Text;
    using AVOne.Tool.Resources;

    [Verb("plugin-repo", false, HelpText = nameof(Resource.HelpTextVerbPluginRepo), ResourceType = typeof(Resource))]
    internal class PluginRepo : BaseHostOptions
    {
        [Option('l', "list", Required = false, Group = "action", HelpText = nameof(Resource.HelpTextShowPluginRepositoryList), ResourceType = typeof(Resource))]
        public bool List { get; set; }

        [Option('s', "show-packages", Required = false, Group = "action", HelpText = nameof(Resource.HelpTextShowAvaliablePlugins), ResourceType = typeof(Resource))]
        public bool Search { get; set; }

        [Option('a', "add-repo", Required = false, Group = "action", Max = 2, Min = 2, HelpText = nameof(Resource.HelpTextAddRepo), ResourceType = typeof(Resource))]
        public IEnumerable<string>? AddRepoOption { get; set; }

        [Usage(ApplicationAlias = ToolAlias)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(Resource.HelpTextAddRepo, new PluginRepo { AddRepoOption = new string[] { "Jellyfin Stable", "https://repo.jellyfin.org/releases/plugin/manifest-stable.json" } });
                yield return new Example(Resource.HelpTextShowAvaliablePlugins, new PluginRepo { Search = true });
                yield return new Example(Resource.HelpTextShowPluginRepositoryList, new PluginRepo { List = true });
            }
        }

        internal override Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                var pluginManager = host.Resolve<IPluginManager>();
                var configurationManager = host.Resolve<IServerConfigurationManager>();
                var installationManager = host.Resolve<IInstallationManager>();
                if (List)
                {

                    ListPluginRepos(configurationManager);
                }
                else if (Search)
                {
                    await SearchPluginsInRepos(installationManager, token);
                }
                else if (AddRepoOption?.Count() == 2)
                {
                    AddRepo(configurationManager);
                }
            }, token);
        }

        private void AddRepo(IServerConfigurationManager configurationManager)
        {
            var repos = configurationManager.Configuration.PluginRepositories;
            var opts = AddRepoOption?.ToArray();
            var name = opts?[0];
            var path = opts?[1];
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            if (repos.Any(e => e.Name == name))
            {
                Cli.Error("Repo Name '{0}' already exists", name);
            }
            else
            {
                var newRepos = repos.ToList();
                newRepos.Add(new RepositoryInfo { Name = name, Url = path, Enabled = true });
                configurationManager.Configuration.PluginRepositories = newRepos.ToArray();
                configurationManager.SaveConfiguration();
                Cli.Success("Repo Name '{0}' added successfully", name);
            }
        }

        private async Task SearchPluginsInRepos(
            IInstallationManager installationManager,
            CancellationToken token)
        {
            var packages = await installationManager.GetAvailablePackages(token);
            var columns = new List<string>
            {
                nameof(PackageInfo.Id), nameof(PackageInfo.Name), nameof(PackageInfo.Description), nameof(PackageInfo.Category)
            };
            var renders = new List<Func<PackageInfo, IRenderable>>();
            renders.AddRange(columns.Select(Cli.Text<PackageInfo>));
            columns.Add(nameof(PackageInfo.Versions));
            renders.Add(Cli.Table<PackageInfo, VersionInfo>((p) => p.Versions.AsEnumerable(), false, nameof(VersionInfo.Version)));
            Cli.PrintTable(packages, columns, renders, true);
        }

        private void ListPluginRepos(IServerConfigurationManager configurationManager)
        {
            var repos = configurationManager.Configuration.PluginRepositories;
            Cli.PrintTable(repos, true, nameof(RepositoryInfo.Name), nameof(RepositoryInfo.Url), nameof(RepositoryInfo.Enabled));
        }
    }
}
