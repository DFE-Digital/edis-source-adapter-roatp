using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Roatp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka
{
    public class KafkaRoatpDataReceiver : IRoatpDataReceiver, IDisposable
    {
        private readonly DataServicePlatformConfiguration _configuration;
        private readonly ILogger<KafkaRoatpDataReceiver> _logger;
        private CachedSchemaRegistryClient _schemaRegistry;
        private readonly IProducer<long, ApprenticeshipProvider> _producer;

        public KafkaRoatpDataReceiver(
            DataServicePlatformConfiguration configuration,
            ILogger<KafkaRoatpDataReceiver> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = configuration.SchemaRegistryUrl,
            };
            _schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
            
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration.KafkaBootstrapServers
            };
            var serializerConfig = new JsonSerializerConfig
            {
            };
            _producer = new ProducerBuilder<long, ApprenticeshipProvider>(producerConfig)
                .SetValueSerializer(new JsonSerializer<ApprenticeshipProvider>(_schemaRegistry, serializerConfig))
                .Build();
        }

        public async Task SendDataAsync(ApprenticeshipProvider provider, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending {UKPRN} to Kafka topic {TopicName}",
                provider.Ukprn, _configuration.RoatpProviderTopic);
            
            _logger.LogDebug($"Expected key schema subject name is: {SubjectNameStrategy.Topic.ConstructKeySubjectName(_configuration.RoatpProviderTopic)}");
            _logger.LogDebug($"Expected value schema subject name is: {SubjectNameStrategy.Topic.ConstructValueSubjectName(_configuration.RoatpProviderTopic)}");
         
            var message = new Message<long, ApprenticeshipProvider>
            {
                Key = provider.Ukprn,
                Value = provider,
            };
            var result = await _producer.ProduceAsync(_configuration.RoatpProviderTopic, message, cancellationToken);
            
            _logger.LogInformation("Message for {UKPRN} stored as offset {Offset} in partition {Partition} for {TopicName}",
                provider.Ukprn, result.Offset, result.Partition, _configuration.RoatpProviderTopic);
        }

        public void Dispose()
        {
            _schemaRegistry?.Dispose();
            _producer?.Dispose();
        }
    }
}