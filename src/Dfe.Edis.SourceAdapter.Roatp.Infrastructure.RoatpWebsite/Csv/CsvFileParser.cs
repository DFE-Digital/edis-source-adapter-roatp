using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace Dfe.Edis.SourceAdapter.Roatp.Infrastructure.RoatpWebsite.Csv
{
    public abstract class CsvFileParser<T> : IDisposable
    {
        private readonly StreamReader _reader;
        private readonly CsvReader _csv;

        protected CsvFileParser(StreamReader reader, ClassMap<T> mapping)
        {
            _reader = reader;
            _csv = new CsvReader(_reader, CultureInfo.CurrentCulture);
            _csv.Configuration.RegisterClassMap(mapping);

            // TODO: Handle missing fields
            // _csv.Configuration.HeaderValidated = null;
            // _csv.Configuration.MissingFieldFound = null;
        }

        public virtual T[] GetRecords()
        {
            return _csv.GetRecords<T>().ToArray();
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _csv?.Dispose();
        }
    }
}