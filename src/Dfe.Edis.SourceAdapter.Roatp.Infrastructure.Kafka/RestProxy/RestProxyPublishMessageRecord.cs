namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka.RestProxy
{
    public class RestProxyPublishMessageRecord<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}