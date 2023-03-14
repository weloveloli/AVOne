// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.
#nullable disable

namespace AVOne.Tool
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Reflection;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using My.Extensions.Localization.Json.Internal;
    using My.Extensions.Localization.Json;

    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly My.Extensions.Localization.Json.Caching.IResourceNamesCache _resourceNamesCache = new My.Extensions.Localization.Json.Caching.ResourceNamesCache();

        private readonly ConcurrentDictionary<string, JsonStringLocalizer> _localizerCache = new ConcurrentDictionary<string, JsonStringLocalizer>();

        private readonly string _resourcesRelativePath;

        private readonly ResourcesType _resourcesType = ResourcesType.TypeBased;

        private readonly ILoggerFactory _loggerFactory;

        public JsonStringLocalizerFactory(IOptions<JsonLocalizationOptions> localizationOptions, ILoggerFactory loggerFactory)
        {
            if (localizationOptions == null)
            {
                throw new ArgumentNullException("localizationOptions");
            }

            _resourcesRelativePath = localizationOptions.Value.ResourcesPath ?? string.Empty;
            _resourcesType = localizationOptions.Value.ResourcesType;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException("loggerFactory");
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
            {
                throw new ArgumentNullException("resourceSource");
            }

            string resourcesPath = string.Empty;
            if (resourceSource.Name == "Controller")
            {
                resourcesPath = Path.Combine(PathHelpers.GetApplicationRoot(), GetResourcePath(resourceSource.Assembly));
                return _localizerCache.GetOrAdd(resourceSource.Name, (string _) => CreateJsonStringLocalizer(resourcesPath, TryFixInnerClassPath("Controller")));
            }

            TypeInfo typeInfo = resourceSource.GetTypeInfo();
            Assembly assembly = typeInfo.Assembly;
            string name = resourceSource.Assembly.GetName().Name;
            string typeName = ((name + "." + typeInfo.Name == typeInfo.FullName) ? typeInfo.Name : TrimPrefix(typeInfo.FullName, name + "."));
            resourcesPath = Path.Combine(PathHelpers.GetApplicationRoot(), GetResourcePath(assembly));
            typeName = TryFixInnerClassPath(typeName);
            return _localizerCache.GetOrAdd("culture=" + CultureInfo.CurrentUICulture.Name + ", typeName=" + typeName, (string _) => CreateJsonStringLocalizer(resourcesPath, typeName));
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException("baseName");
            }

            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            return _localizerCache.GetOrAdd("baseName=" + baseName + ",location=" + location, delegate
            {
                Assembly assembly = Assembly.Load(new AssemblyName(location));

#if RELEASE
                string resourcesPath = Path.Combine(StartupHelpers.RealRootContentPath, GetResourcePath(assembly));
#else
                string resourcesPath = Path.Combine(PathHelpers.GetApplicationRoot(), GetResourcePath(assembly));
#endif
                string resourceName = null;
                if (baseName == string.Empty)
                {
                    resourceName = baseName;
                    return CreateJsonStringLocalizer(resourcesPath, resourceName);
                }

                if (_resourcesType == ResourcesType.TypeBased)
                {
                    baseName = TryFixInnerClassPath(baseName);
                    resourceName = TrimPrefix(baseName, location + ".");
                }

                return CreateJsonStringLocalizer(resourcesPath, resourceName);
            });
        }

        protected virtual JsonStringLocalizer CreateJsonStringLocalizer(string resourcesPath, string resourceName)
        {
            return new JsonStringLocalizer((_resourcesType == ResourcesType.TypeBased) ? new JsonResourceManager(resourcesPath, resourceName) : new JsonResourceManager(resourcesPath), logger: _loggerFactory.CreateLogger<JsonStringLocalizer>(), resourceNamesCache: _resourceNamesCache);
        }

        private string GetResourcePath(Assembly assembly)
        {
            ResourceLocationAttribute customAttribute = assembly.GetCustomAttribute<ResourceLocationAttribute>();
            if (customAttribute != null)
            {
                return customAttribute.ResourceLocation;
            }
            return _resourcesRelativePath;
        }

        private static string TrimPrefix(string name, string prefix)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return name.Substring(prefix.Length);
            }

            return name;
        }

        private string TryFixInnerClassPath(string path)
        {
            string result = path;
            if (path.Contains('+'.ToString()))
            {
                result = path.Replace('+', '.');
            }

            return result;
        }
    }
}

