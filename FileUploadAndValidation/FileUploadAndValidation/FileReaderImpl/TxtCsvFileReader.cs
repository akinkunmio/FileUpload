using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Tokenizer;

namespace FileUploadAndValidation.FileReaders
{
    public class TxtCsvFileReader<T> : ITxtCsvReader<T> where T : class
    {
        public TxtCsvFileReader()
        { }
        public async Task<List<CsvMappingResult<T>>> Read(byte[] fileBytes, ICsvMapping<T> csvMapping)
        {
            var csvReaderOptions = new CsvReaderOptions(new[] { Environment.NewLine });
            var csvParserOptions = new CsvParserOptions(false, new QuotedStringTokenizer(','));
            using (var memoryStream = new MemoryStream(fileBytes))
            {
                var csvParser = new CsvParser<T>(csvParserOptions, csvMapping);
                var stringifiedStream = Encoding.UTF8.GetString(memoryStream.ToArray());
                return await Task.FromResult(csvParser.ReadFromString(csvReaderOptions, stringifiedStream).ToList());
            }
        }
    }

    public interface ITxtCsvReader<T> where T : class
    {
        Task<List<CsvMappingResult<T>>> Read(byte[] fileBytes, ICsvMapping<T> csvMapping);
    }
}
