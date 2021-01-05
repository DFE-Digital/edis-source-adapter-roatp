namespace Dfe.Edis.SourceAdapter.Roatp.Domain.Configuration
{
    public class RootAppConfiguration
    {
        public SourceDataConfiguration SourceData { get; set; }
        public DataServicePlatformConfiguration DataServicePlatform { get; set; }
        public StateConfiguration State { get; set; }
    }
}