using System;
using Newtonsoft.Json;

namespace Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp
{
    public class ApprenticeshipProvider
    {
        [JsonProperty("ukprn")]
        public long Ukprn { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("providerType")]
        public string ProviderType { get; set; }
        
        [JsonProperty("parentCompanyGuarantee")]
        public bool ParentCompanyGuarantee { get; set; }
        
        [JsonProperty("newOrganisationWithoutFinancialTrackRecord")]
        public bool NewOrganisationWithoutFinancialTrackRecord { get; set; }
        
        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }
        
        [JsonProperty("providerNotCurrentlyStartingNewApprentices")]
        public bool ProviderNotCurrentlyStartingNewApprentices { get; set; }
        
        [JsonProperty("applicationDeterminedDate")]
        public DateTime ApplicationDeterminedDate { get; set; }
    }
}