using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.Kafka.Producer;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Roatp.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka
{
    public class KafkaRoatpDataReceiver : IRoatpDataReceiver, IDisposable
    {
        private readonly IKafkaProducer<string, ApprenticeshipProvider> _producer;
        private readonly DataServicePlatformConfiguration _configuration;
        private readonly ILogger<KafkaRoatpDataReceiver> _logger;

        public KafkaRoatpDataReceiver(
            IKafkaProducer<string, ApprenticeshipProvider> producer,
            DataServicePlatformConfiguration configuration,
            ILogger<KafkaRoatpDataReceiver> logger)
        {
            _producer = producer;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendDataAsync(ApprenticeshipProvider provider, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending {UKPRN} to Kafka topic {TopicName}",
                provider.Ukprn, _configuration.RoatpProviderTopic);

            var result = await _producer.ProduceAsync(
                _configuration.RoatpProviderTopic,
                provider.Ukprn.ToString(),
                provider,
                cancellationToken);

            _logger.LogInformation("Message for {UKPRN} stored as offset {Offset} in partition {Partition} for {TopicName}",
                provider.Ukprn, result.Offset, result.Partition, _configuration.RoatpProviderTopic);
        }

        public void Dispose()
        {
        }
    }
}