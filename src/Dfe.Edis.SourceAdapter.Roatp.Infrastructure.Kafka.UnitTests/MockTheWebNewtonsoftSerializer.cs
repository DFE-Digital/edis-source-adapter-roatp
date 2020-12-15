using Newtonsoft.Json;

namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.Kafka.UnitTests
{
    public class MockTheWebNewtonsoftSerializer : MockTheWeb.IJsonSerializer
    {
        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item);
        }
    }
}