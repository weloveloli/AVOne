// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

using AVOne.Impl;
using AVOne.Server;
using CommandLine;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
        var parsedResult = Parser.Default.ParseArguments<ServerHostOptions>(args);
        if (parsedResult is not Parsed<ServerHostOptions> parsed)
        {
            Environment.Exit(1);
            return;
        }

        var option = parsed.Value;
        var builder = WebApplication.CreateBuilder(args).Inject();
        var appPaths = StartupHelpers.CreateApplicationPaths(option!);
        var appHost = StartupHelpers.CreateConsoleAppHost(option!, appPaths).Result;
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
        appHost.Init(builder.Services);
        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddMasaBlazor(builder =>
        {
            builder.ConfigureTheme(theme =>
            {
                theme.Themes.Light.Primary = "#4318FF";
                theme.Themes.Light.Accent = "#4318FF";
            });
        }).AddI18nForServer("wwwroot/i18n");
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddGlobalForServer();
        builder.Services.AddControllers().AddInject();
        builder.Services.AddHealthChecks().AddCheck<SampleHealthCheck>("Sample");
        var app = builder.Build();

        // Re-use the host service provider in the app host since ASP.NET doesn't allow a custom service collection.
        appHost.ServiceProvider = app.Services;
        appHost.PostBuildService();
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();
        app.MapHealthChecks("/health");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseInject();
        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
        app.Run();
    }
}
