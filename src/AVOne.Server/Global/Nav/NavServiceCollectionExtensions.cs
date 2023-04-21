// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection;

public static class NavServiceCollectionExtensions
{
    public static IServiceCollection AddNav(this IServiceCollection services, List<NavModel> navList)
    {
        services.AddSingleton(navList);
        services.AddScoped<NavHelper>();

        return services;
    }

    public static IServiceCollection AddNav(this IServiceCollection services, string navSettingsFile)
    {
        var navList = JsonSerializer.Deserialize<List<NavModel>>(File.ReadAllText(navSettingsFile));

        if (navList is null) throw new Exception("Please configure the navigation first!");

        services.AddNav(navList);

        return services;
    }
}

