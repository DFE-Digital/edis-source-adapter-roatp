using System.IO;
using CsvHelper.Configuration;
using Dfe.Edis.SourceAdapter.Roatp.Domain.Roatp;

namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.RoatpWebsite.Csv
{
    public class PublishedRoatpCsvParser : CsvFileParser<ApprenticeshipProvider>
    {
        private class PublishedRoatpCsv : ClassMap<ApprenticeshipProvider>
        {
            public PublishedRoatpCsv()
            {
                var optionalBoolConverter = new OptionalBoolConverter();
                var dateTimeConverter = new DateTimeConverter();
                
                Map(x => x.Ukprn)
                    .Name("Ukprn");
                
                Map(x => x.Name)
                    .Name("Name");
                
                Map(x => x.ProviderType)
                    .Name("ProviderType");
                
                Map(x => x.ParentCompanyGuarantee)
                    .Name("ParentCompanyGuarantee")
                    .TypeConverter(optionalBoolConverter);
                
                Map(x => x.NewOrganisationWithoutFinancialTrackRecord)
                    .Name("NewOrganisationWithoutFinancialTrackRecord")
                    .TypeConverter(optionalBoolConverter);
                
                Map(x => x.StartDate)
                    .Name("StartDate")
                    .TypeConverter(dateTimeConverter);
                
                Map(x => x.ProviderNotCurrentlyStartingNewApprentices)
                    .Name("ProviderNotCurrentlyStartingNewApprentices")
                    .TypeConverter(optionalBoolConverter);

                Map(x => x.ApplicationDeterminedDate)
                    .Name("ApplicationDeterminedDate")
                    .TypeConverter(dateTimeConverter);
            }
        }
        
        public PublishedRoatpCsvParser(StreamReader reader) 
            : base(reader, new PublishedRoatpCsv())
        {
        }
    }
}