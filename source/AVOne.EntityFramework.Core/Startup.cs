﻿using Furion;
using Microsoft.Extensions.DependencyInjection;

namespace AVOne.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "AVOne.Database.Migrations");
    }
}
