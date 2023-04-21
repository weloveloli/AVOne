// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Extensions
{
    using Furion;
    using Furion.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultHostLocalization(this IServiceCollection services)
        {
            var localizationSettings = App.GetConfig<LocalizationSettingsOptions>("LocalizationSettings", true);
            services.AddConfigurableOptions<LocalizationSettingsOptions>();
            services.AddJsonLocalization(options =>
            {
                if (!string.IsNullOrWhiteSpace(localizationSettings.ResourcesPath))
                    options.ResourcesPath = localizationSettings.ResourcesPath;
            });
            return services;
        }
    }
}
