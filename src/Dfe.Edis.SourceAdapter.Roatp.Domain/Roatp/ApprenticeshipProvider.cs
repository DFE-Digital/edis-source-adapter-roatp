using System;

namespace Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp
{
    public class ApprenticeshipProvider
    {
        public long Ukprn { get; set; }
        public string Name { get; set; }
        public string ProviderType { get; set; }
        public bool ParentCompanyGuarantee { get; set; }
        public bool NewOrganisationWithoutFinancialTrackRecord { get; set; }
        public DateTime StartDate { get; set; }
        public bool ProviderNotCurrentlyStartingNewApprentices { get; set; }
        public DateTime ApplicationDeterminedDate { get; set; }
    }
}