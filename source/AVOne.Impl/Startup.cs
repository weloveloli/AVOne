namespace AVOne.Impl
{
    using AVOne.Impl.Facade;
    using Furion;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup : AppStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMetaDataFacade, MetaDataFacade>();
        }
    }
}
