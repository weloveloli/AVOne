// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Reflection;
    using System.Text;
    using Emby.Server.Implementations;
    using Emby.Server.Implementations.Session;
    using Jellyfin.Api.WebSocketListeners;
    using Jellyfin.Drawing;
    using Jellyfin.Drawing.Skia;
    using Jellyfin.Server.Implementations;
    using Jellyfin.Server.Implementations.Activity;
    using Jellyfin.Server.Implementations.Devices;
    using Jellyfin.Server.Implementations.Events;
    using Jellyfin.Server.Implementations.Extensions;
    using Jellyfin.Server.Implementations.Security;
    using Jellyfin.Server.Implementations.Users;
    using MediaBrowser.Common.Net;
    using MediaBrowser.Controller;
    using MediaBrowser.Controller.BaseItemManager;
    using MediaBrowser.Controller.Devices;
    using MediaBrowser.Controller.Drawing;
    using MediaBrowser.Controller.Events;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Controller.Lyrics;
    using MediaBrowser.Controller.Net;
    using MediaBrowser.Controller.Security;
    using MediaBrowser.Model.Activity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class ConsoleAppHost : ApplicationHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAppHost" /> class.
        /// </summary>
        /// <param name="applicationPaths">The <see cref="ServerApplicationPaths" /> to be used by the <see cref="CoreAppHost" />.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory" /> to be used by the <see cref="CoreAppHost" />.</param>
        /// <param name="options">The <see cref="StartupOptions" /> to be used by the <see cref="CoreAppHost" />.</param>
        /// <param name="startupConfig">The <see cref="IConfiguration" /> to be used by the <see cref="CoreAppHost" />.</param>
        public ConsoleAppHost(
            IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            IStartupOptions options,
            IConfiguration startupConfig)
            : base(
                applicationPaths,
                loggerFactory,
                options,
                startupConfig)
        {
        }

        /// <inheritdoc/>
        protected override void RegisterServices(IServiceCollection serviceCollection)
        {
            // Register an image encoder
            bool useSkiaEncoder = SkiaEncoder.IsNativeLibAvailable();
            Type imageEncoderType = useSkiaEncoder
                ? typeof(SkiaEncoder)
                : typeof(NullImageEncoder);
            serviceCollection.AddSingleton(typeof(IImageEncoder), imageEncoderType);

            // Log a warning if the Skia encoder could not be used
            if (!useSkiaEncoder)
            {
                Logger.LogWarning("Skia not available. Will fallback to {ImageEncoder}.", nameof(NullImageEncoder));
            }

            serviceCollection.AddEventServices();
            serviceCollection.AddSingleton<IBaseItemManager, BaseItemManager>();
            serviceCollection.AddSingleton<IEventManager, EventManager>();

            serviceCollection.AddSingleton<IActivityManager, ActivityManager>();
            serviceCollection.AddSingleton<IUserManager, UserManager>();
            serviceCollection.AddScoped<IDisplayPreferencesManager, DisplayPreferencesManager>();
            serviceCollection.AddSingleton<IDeviceManager, DeviceManager>();

            // TODO search the assemblies instead of adding them manually?
            serviceCollection.AddSingleton<IWebSocketListener, SessionWebSocketListener>();
            serviceCollection.AddSingleton<IWebSocketListener, ActivityLogWebSocketListener>();
            serviceCollection.AddSingleton<IWebSocketListener, ScheduledTasksWebSocketListener>();
            serviceCollection.AddSingleton<IWebSocketListener, SessionInfoWebSocketListener>();

            serviceCollection.AddSingleton<IAuthorizationContext, AuthorizationContext>();

            serviceCollection.AddScoped<IAuthenticationManager, Jellyfin.Server.Implementations.Security.AuthenticationManager>();

            foreach (var type in GetExportTypes<ILyricProvider>())
            {
                serviceCollection.AddSingleton(typeof(ILyricProvider), type);
            }

            base.RegisterServices(serviceCollection);
            serviceCollection.AddJellyfinDbContext();
            var productHeader = new ProductInfoHeaderValue(
            this.Name.Replace(' ', '-'),
            this.ApplicationVersionString);
            var acceptJsonHeader = new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json, 1.0);
            var acceptXmlHeader = new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Xml, 0.9);
            var acceptAnyHeader = new MediaTypeWithQualityHeaderValue("*/*", 0.8);
            Func<IServiceProvider, HttpMessageHandler> defaultHttpClientHandlerDelegate = (_) => new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.All,
                RequestHeaderEncodingSelector = (_, _) => Encoding.UTF8
            };
            serviceCollection.AddHttpClient(NamedClient.Default, c =>
            {
                c.DefaultRequestHeaders.UserAgent.Add(productHeader);
                c.DefaultRequestHeaders.Accept.Add(acceptJsonHeader);
                c.DefaultRequestHeaders.Accept.Add(acceptXmlHeader);
                c.DefaultRequestHeaders.Accept.Add(acceptAnyHeader);
            })
                .ConfigurePrimaryHttpMessageHandler(defaultHttpClientHandlerDelegate);

            serviceCollection.AddHttpClient(NamedClient.MusicBrainz, c =>
            {
                c.DefaultRequestHeaders.UserAgent.Add(productHeader);
                c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"({this.ApplicationUserAgentAddress})"));
                c.DefaultRequestHeaders.Accept.Add(acceptXmlHeader);
                c.DefaultRequestHeaders.Accept.Add(acceptAnyHeader);
            })
                .ConfigurePrimaryHttpMessageHandler(defaultHttpClientHandlerDelegate);

            serviceCollection.AddHttpClient(NamedClient.Dlna, c =>
            {
                c.DefaultRequestHeaders.UserAgent.ParseAdd(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}/{1} UPnP/1.0 {2}/{3}",
                        Environment.OSVersion.Platform,
                        Environment.OSVersion,
                        this.Name,
                        this.ApplicationVersionString));

                c.DefaultRequestHeaders.Add("CPFN.UPNP.ORG", this.FriendlyName); // Required for UPnP DeviceArchitecture v2.0
                c.DefaultRequestHeaders.Add("FriendlyName.DLNA.ORG", this.FriendlyName); // REVIEW: where does this come from?
            })
                .ConfigurePrimaryHttpMessageHandler(defaultHttpClientHandlerDelegate);
        }

        /// <inheritdoc />
        protected override void RestartInternal() => StartupHelpers.Restart();

        /// <inheritdoc />
        protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
        {
            // Jellyfin.Server
            yield return typeof(ConsoleAppHost).Assembly;

            // Jellyfin.Server.Implementations
            yield return typeof(JellyfinDbContext).Assembly;
        }

        /// <inheritdoc />
        protected override void ShutdownInternal() => StartupHelpers.Shutdown();
    }
}
