namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka.RestProxy
{
    public class RestProxyPublishMessage<TKey, TValue>
    {
        public RestProxyPublishMessageRecord<TKey, TValue>[] Records { get; set; }
    }
}