using System.IO;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Roatp.FunctionApp;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Dfe.Edis.SourceAdapter.Roatp.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var rawConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();

            Configure(builder, rawConfiguration);
        }
        public void Configure(IFunctionsHostBuilder builder, IConfigurationRoot configurationRoot)
        {
            var services = builder.Services;

            AddConfiguration(services, configurationRoot);
            AddLogging(services);
        }


        private void AddConfiguration(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            var configuration = new RootAppConfiguration();
            configurationRoot.Bind(configuration);
            
            
            services.AddSingleton(configurationRoot);

            services.AddSingleton(configuration);
        }

        private void AddLogging(IServiceCollection services)
        {
            services.AddLogging();
        }
    }
}