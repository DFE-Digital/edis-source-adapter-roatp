using System.IO;
using Confluent.Kafka;
using Dfe.Edis.SourceAdapter.Roatp.Application;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Roatp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;
using Dfe.Edis.SourceAdapter.Roatp.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Roatp.FunctionApp;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.AzureStorage;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka.RestProxy;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.RoatpWebsite;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

            JsonConvert.DefaultSettings = () =>
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

            AddConfiguration(services, configurationRoot);
            AddLogging(services);

            AddRoatpDataSource(services);
            AddRoatpDataReceiver(services);
            AddState(services);

            services
                .AddScoped<IChangeProcessor, ChangeProcessor>();
        }


        private void AddConfiguration(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            var configuration = new RootAppConfiguration();
            configurationRoot.Bind(configuration);


            services.AddSingleton(configurationRoot);

            services.AddSingleton(configuration);
            services.AddSingleton(configuration.SourceData);
            services.AddSingleton(configuration.DataServicePlatform);
            services.AddSingleton(configuration.State);
        }

        private void AddLogging(IServiceCollection services)
        {
            services.AddLogging(builder => { builder.SetMinimumLevel(LogLevel.Debug); });
        }

        private void AddRoatpDataSource(IServiceCollection services)
        {
            services.AddHttpClient<RoatpWebsiteDataSource>();
            services.AddScoped<IRoatpDataSource, RoatpWebsiteDataSource>();
        }

        private void AddRoatpDataReceiver(IServiceCollection services)
        {
            services.AddHttpClient<KafkaRestProxyRoatpDataReceiver>();
            services.AddScoped<IRoatpDataReceiver, KafkaRestProxyRoatpDataReceiver>();
        }

        private void AddState(IServiceCollection services)
        {
            services.AddScoped<IStateStore, BlobStateStore>();
        }
    }
}