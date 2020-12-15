namespace Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration
{
    public class DataServicePlatformConfiguration
    {
        public string KafkaBootstrapServers { get; set; }
        public string SchemaRegistryUrl { get; set; }
        public string RoatpProviderTopic { get; set; }
    }
}