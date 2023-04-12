// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Api.Controllers
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json;
    using AVOne.Api.Attributes;
    using AVOne.Common;
    using AVOne.Common.Plugins;
    using AVOne.Impl.Json;
    using AVOne.Updates;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controllers for plugins
    /// </summary>
    /// <seealso cref="AVOne.Api.Controllers.BaseApiController" />
    public class PluginsController : BaseApiController
    {
        private readonly IInstallationManager _installationManager;
        private readonly IPluginManager _pluginManager;
        private readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginsController"/> class.
        /// </summary>
        /// <param name="installationManager">Instance of the <see cref="IInstallationManager"/> interface.</param>
        /// <param name="pluginManager">Instance of the <see cref="IPluginManager"/> interface.</param>
        public PluginsController(
            IInstallationManager installationManager,
            IPluginManager pluginManager)
        {
            _installationManager = installationManager;
            _pluginManager = pluginManager;
            _serializerOptions = JsonDefaults.Options;
        }

        /// <summary>
        /// Gets a list of currently installed plugins.
        /// </summary>
        /// <response code="200">Installed plugins returned.</response>
        /// <returns>List of currently installed plugins.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<PluginInfo>> GetPlugins()
        {
            return Ok(_pluginManager.Plugins
                .OrderBy(p => p.Name)
                .Select(p => p.GetPluginInfo()));
        }

        /// <summary>
        /// Enables a disabled plugin.
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <param name="version">Plugin version.</param>
        /// <response code="204">Plugin enabled.</response>
        /// <response code="404">Plugin not found.</response>
        /// <returns>An <see cref="NoContentResult"/> on success, or a <see cref="NotFoundResult"/> if the plugin could not be found.</returns>
        [HttpPost("{pluginId}/{version}/Enable")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult EnablePlugin([FromRoute, Required] Guid pluginId, [FromRoute, Required] Version version)
        {
            var plugin = _pluginManager.GetPlugin(pluginId, version);
            if (plugin == null)
            {
                return NotFound();
            }

            _pluginManager.EnablePlugin(plugin);
            return NoContent();
        }

        /// <summary>
        /// Disable a plugin.
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <param name="version">Plugin version.</param>
        /// <response code="204">Plugin disabled.</response>
        /// <response code="404">Plugin not found.</response>
        /// <returns>An <see cref="NoContentResult"/> on success, or a <see cref="NotFoundResult"/> if the plugin could not be found.</returns>
        [HttpPost("{pluginId}/{version}/Disable")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DisablePlugin([FromRoute, Required] Guid pluginId, [FromRoute, Required] Version version)
        {
            var plugin = _pluginManager.GetPlugin(pluginId, version);
            if (plugin == null)
            {
                return NotFound();
            }

            _pluginManager.DisablePlugin(plugin);
            return NoContent();
        }

        /// <summary>
        /// Uninstalls a plugin by version.
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <param name="version">Plugin version.</param>
        /// <response code="204">Plugin uninstalled.</response>
        /// <response code="404">Plugin not found.</response>
        /// <returns>An <see cref="NoContentResult"/> on success, or a <see cref="NotFoundResult"/> if the plugin could not be found.</returns>
        [HttpDelete("{pluginId}/{version}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UninstallPluginByVersion([FromRoute, Required] Guid pluginId, [FromRoute, Required] Version version)
        {
            var plugin = _pluginManager.GetPlugin(pluginId, version);
            if (plugin == null)
            {
                return NotFound();
            }

            _installationManager.UninstallPlugin(plugin);
            return NoContent();
        }

        /// <summary>
        /// Uninstalls a plugin.
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <response code="204">Plugin uninstalled.</response>
        /// <response code="404">Plugin not found.</response>
        /// <returns>An <see cref="NoContentResult"/> on success, or a <see cref="NotFoundResult"/> if the plugin could not be found.</returns>
        [HttpDelete("{pluginId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Obsolete("Please use the UninstallPluginByVersion API.")]
        public ActionResult UninstallPlugin([FromRoute, Required] Guid pluginId)
        {
            // If no version is given, return the current instance.
            var plugins = _pluginManager.Plugins.Where(p => p.Id.Equals(pluginId));

            // Select the un-instanced one first.
            var plugin = plugins.FirstOrDefault(p => p.Instance == null) ?? plugins.OrderBy(p => p.Manifest.Status).FirstOrDefault();

            if (plugin != null)
            {
                _installationManager.UninstallPlugin(plugin);
                return NoContent();
            }

            return NotFound();
        }

        /// <summary>
        /// Gets plugin configuration.
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <response code="200">Plugin configuration returned.</response>
        /// <response code="404">Plugin not found or plugin configuration not found.</response>
        /// <returns>Plugin configuration.</returns>
        [HttpGet("{pluginId}/Configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BasePluginConfiguration> GetPluginConfiguration([FromRoute, Required] Guid pluginId)
        {
            var plugin = _pluginManager.GetPlugin(pluginId);
            if (plugin?.Instance is IHasPluginConfiguration configPlugin)
            {
                return configPlugin.Configuration;
            }

            return NotFound();
        }

        /// <summary>
        /// Updates plugin configuration.
        /// </summary>
        /// <remarks>
        /// Accepts plugin configuration as JSON body.
        /// </remarks>
        /// <param name="pluginId">Plugin id.</param>
        /// <response code="204">Plugin configuration updated.</response>
        /// <response code="404">Plugin not found or plugin does not have configuration.</response>
        /// <returns>An <see cref="NoContentResult"/> on success, or a <see cref="NotFoundResult"/> if the plugin could not be found.</returns>
        [HttpPost("{pluginId}/Configuration")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdatePluginConfiguration([FromRoute, Required] Guid pluginId)
        {
            var plugin = _pluginManager.GetPlugin(pluginId);
            if (plugin?.Instance is not IHasPluginConfiguration configPlugin)
            {
                return NotFound();
            }

            var configuration = (BasePluginConfiguration?)await JsonSerializer.DeserializeAsync(Request.Body, configPlugin.ConfigurationType, _serializerOptions)
                .ConfigureAwait(false);

            if (configuration != null)
            {
                configPlugin.UpdateConfiguration(configuration);
            }

            return NoContent();
        }

        /// <summary>
        /// Gets a plugin's image.
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <param name="version">Plugin version.</param>
        /// <response code="200">Plugin image returned.</response>
        /// <returns>Plugin's image.</returns>
        [HttpGet("{pluginId}/{version}/Image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesImageFile]
        [AllowAnonymous]
        public ActionResult GetPluginImage([FromRoute, Required] Guid pluginId, [FromRoute, Required] Version version)
        {
            var plugin = _pluginManager.GetPlugin(pluginId, version);
            if (plugin == null)
            {
                return NotFound();
            }

            var imagePath = Path.Combine(plugin.Path, plugin.Manifest.ImagePath ?? string.Empty);
            if (plugin.Manifest.ImagePath == null || !System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            imagePath = Path.Combine(plugin.Path, plugin.Manifest.ImagePath);
            return PhysicalFile(imagePath, MimeTypes.GetMimeType(imagePath));
        }

        /// <summary>
        /// Gets a plugin's manifest.
        /// </summary>
        /// <param name="pluginId">Plugin id.</param>
        /// <response code="204">Plugin manifest returned.</response>
        /// <response code="404">Plugin not found.</response>
        /// <returns>A <see cref="PluginManifest"/> on success, or a <see cref="NotFoundResult"/> if the plugin could not be found.</returns>
        [HttpPost("{pluginId}/Manifest")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<PluginManifest> GetPluginManifest([FromRoute, Required] Guid pluginId)
        {
            var plugin = _pluginManager.GetPlugin(pluginId);

            if (plugin != null)
            {
                return plugin.Manifest;
            }

            return NotFound();
        }
    }
}
