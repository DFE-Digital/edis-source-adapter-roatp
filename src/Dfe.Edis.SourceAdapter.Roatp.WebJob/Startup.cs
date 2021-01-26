using System.Net.Http;
using Dfe.Edis.Kafka;
using Dfe.Edis.SourceAdapter.Roatp.Application;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Roatp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;
using Dfe.Edis.SourceAdapter.Roatp.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.AzureStorage;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka.RestProxy;
using Dfe.Edis.SourceAdapter.Roatp.Infrastructure.RoatpWebsite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Dfe.Edis.SourceAdapter.Roatp.FunctionApp
{
    public class Startup
    {

        public void Configure(IServiceCollection services, RootAppConfiguration configuration)
        {
            JsonConvert.DefaultSettings = () =>
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

            services.AddHttpClient();
            services.AddKafkaProducer();

            AddConfiguration(services, configuration);
            AddLogging(services);

            AddRoatpDataSource(services);
            AddRoatpDataReceiver(services, configuration);
            AddState(services);

            services
                .AddScoped<IChangeProcessor, ChangeProcessor>();
        }


        private void AddConfiguration(IServiceCollection services, RootAppConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(configuration.SourceData);
            services.AddSingleton(configuration.DataServicePlatform);
            services.AddSingleton(configuration.State);

            services.AddSingleton(new KafkaBrokerConfiguration
            {
                BootstrapServers = configuration.DataServicePlatform.KafkaBootstrapServers,
            });
            services.AddSingleton(new KafkaSchemaRegistryConfiguration
            {
                BaseUrl = configuration.DataServicePlatform.SchemaRegistryUrl,
            });
        }

        private void AddLogging(IServiceCollection services)
        {
            services.AddLogging(builder => { builder.SetMinimumLevel(LogLevel.Debug); });
        }

        private void AddRoatpDataSource(IServiceCollection services)
        {
            // Having issues with Typed clients with HTTP extensions. Doing this for now
            services.AddScoped<IRoatpDataSource, RoatpWebsiteDataSource>(sp =>
            {
                var httpClientFactory = sp.GetService<IHttpClientFactory>();
                return new RoatpWebsiteDataSource(
                    httpClientFactory.CreateClient(),
                    sp.GetService<IStateStore>(),
                    sp.GetService<SourceDataConfiguration>(),
                    sp.GetService<ILogger<RoatpWebsiteDataSource>>());
            });
        }

        private void AddRoatpDataReceiver(IServiceCollection services, RootAppConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.DataServicePlatform.KafkaBootstrapServers))
            {
                services.AddScoped<IRoatpDataReceiver, KafkaRoatpDataReceiver>();
            }
            else
            {
                // Having issues with Typed clients with HTTP extensions. Doing this for now
                services.AddScoped<IRoatpDataReceiver, KafkaRestProxyRoatpDataReceiver>(sp =>
                {
                    var httpClientFactory = sp.GetService<IHttpClientFactory>();
                    return new KafkaRestProxyRoatpDataReceiver(
                        httpClientFactory.CreateClient(),
                        sp.GetService<DataServicePlatformConfiguration>(),
                        sp.GetService<ILogger<KafkaRestProxyRoatpDataReceiver>>());
                });
            }
        }

        private void AddState(IServiceCollection services)
        {
            services.AddScoped<IStateStore, BlobStateStore>();
        }
    }
}