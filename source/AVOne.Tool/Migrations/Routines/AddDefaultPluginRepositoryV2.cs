namespace AVOne.Tools.Migrations.Routines
{
    using AVOne.Tool;
    using AVOne.Tool.Resources;
    using Emby.Server.Implementations.Updates;
    using Jellyfin.Server.Migrations;
    using MediaBrowser.Common.Updates;
    using MediaBrowser.Controller.Configuration;
    using MediaBrowser.Model.Updates;
    using Spectre.Console;

    /// <summary>
    /// Migration to initialize system configuration with the default plugin repository.
    /// </summary>
    public class AddDefaultPluginRepositoryV2 : IMigrationRoutine
    {
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly IInstallationManager _installationManager;
        private readonly RepositoryInfo[] _defaultRepositoryInfo = new[]
        {
           new RepositoryInfo
           {
                Name = "Jellyfin Stable",
                Url = "https://repo.jellyfin.org/releases/plugin/manifest-stable.json"
           },
           new RepositoryInfo
           {
                Name = "MetaTube",
                Url = "https://cdn.jsdelivr.net/gh/metatube-community/jellyfin-plugin-metatube@dist/manifest.json"
           }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AddDefaultPluginRepository"/> class.
        /// </summary>
        /// <param name="serverConfigurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        public AddDefaultPluginRepositoryV2(IServerConfigurationManager serverConfigurationManager, IInstallationManager installationManager)
        {
            _serverConfigurationManager = serverConfigurationManager;
            _installationManager = installationManager;
        }

        /// <inheritdoc/>
        public Guid Id => Guid.Parse("EB58EBEE-9514-4B9B-8225-12E1A40020DF");

        /// <inheritdoc/>
        public string Name => "AddDefaultPluginRepositoryV2";

        /// <inheritdoc/>
        public bool PerformOnNewInstall => true;

        /// <inheritdoc/>
        public void Perform()
        {
            _serverConfigurationManager.Configuration.PluginRepositories = _defaultRepositoryInfo;
            _serverConfigurationManager.SaveConfiguration();
            var name = "MetaTube";
            var version = "latest";
            var AddPluginOption = $"{name}@{version}";
            // Asynchronous
            AnsiConsole.Status()
                .Start(Resource.InfoSearchingPlugins, ctx =>
                {
                    var packages = _installationManager.GetAvailablePackages(default).Result;
                    var packagesToInstall = _installationManager.GetCompatibleVersions(packages, name: name);
                    if (version != "latest")
                    {
                        packagesToInstall = _installationManager.GetCompatibleVersions(packages, name: name, specificVersion: Version.Parse(version));
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
                        _installationManager.InstallPackage(package, default).Wait();
                        ctx.Status(string.Format(Resource.SuccessDownloadPlugin, AddPluginOption));
                        Cli.Success(Resource.SuccessDownloadPlugin, AddPluginOption);
                    }
                    catch(Exception e)
                    {
                        Cli.Exception(e, "install package failed", e);
                    }
                });
        }
    }
}
