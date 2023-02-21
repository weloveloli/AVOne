// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Configuration
{
    public interface IConfigurationManager
    {
        /// <summary>
        /// Saves the configuration.
        /// </summary>
        void SaveConfiguration();

        /// <summary>
        /// Gets the application paths.
        /// </summary>
        /// <value>The application paths.</value>
        IApplicationPaths ApplicationPaths { get; }

        /// <summary>
        /// Gets the common configuration.
        /// </summary>
        /// <value>
        /// The common configuration.
        /// </value>
        BaseApplicationConfiguration CommonConfiguration { get; }

        /// <summary>
        /// Replaces the configuration.
        /// </summary>
        /// <param name="newConfiguration">The new configuration.</param>
        void ReplaceConfiguration(BaseApplicationConfiguration newConfiguration);

        /// <summary>
        /// Manually pre-loads a factory so that it is available pre system initialisation.
        /// </summary>
        /// <typeparam name="T">Class to register.</typeparam>
        void RegisterConfiguration<T>()
            where T : IConfigurationFactory;

        /// <summary>
        /// Gets the array of coniguration stores.
        /// </summary>
        /// <returns>Array of ConfigurationStore.</returns>
        ConfigurationStore[] GetConfigurationStores();

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.Object.</returns>
        object GetConfiguration(string key);

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="configuration">The configuration.</param>
        void SaveConfiguration(string key, object configuration);

        /// <summary>
        /// Adds the parts.
        /// </summary>
        /// <param name="factories">The factories.</param>
        void AddParts(IEnumerable<IConfigurationFactory> factories);
    }

    public static class ConfigurationManagerExtensions
    {
        public static T GetConfiguration<T>(this IConfigurationManager manager, string key)
        {
            return (T)manager.GetConfiguration(key);
        }
    }
}
