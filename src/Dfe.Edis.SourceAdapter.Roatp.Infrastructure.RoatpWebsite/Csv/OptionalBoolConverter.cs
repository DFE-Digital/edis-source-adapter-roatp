using System;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.RoatpWebsite.Csv
{
    public class OptionalBoolConverter : DefaultTypeConverter
    {
        private static readonly string[] TrueValues = new[] {"true", "yes", "1"};
        
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return TrueValues.Any(v => v.Equals(text, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}