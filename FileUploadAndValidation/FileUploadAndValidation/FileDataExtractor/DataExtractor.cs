using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using ExcelMapper;
using EPPlus.Core.Extensions.Attributes;
using OfficeOpenXml;
using EPPlus.Core.Extensions;

namespace FileUploadAndValidation.FileDataExtractor
{
    public class DataExtractor<T> : IDataExtractor<T> where T : class
    {

        public DataExtractor()
        {          
        }
        public async Task<IList<T>> ExtractDataFromTxtCsvFile(byte[] fileBytes, ICsvMapping<T> csvMapping)
        {
            var records = new List<T>();
            var mappingResultList = new List<CsvMappingResult<T>>();

            using (var memoryStream = new MemoryStream(fileBytes))
            {
                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';');
                CsvReaderOptions csvReaderOptions = new CsvReaderOptions(new[] { Environment.NewLine });

                var csvParser = new CsvParser<T>(csvParserOptions, csvMapping);

                var stringifiedStream = Encoding.UTF8.GetString(memoryStream.ToArray());

                mappingResultList = await Task.FromResult(csvParser.ReadFromString(csvReaderOptions, stringifiedStream).ToList());

                mappingResultList.ForEach(e => records.Add(e.Result));
            }

            return records;
        }

        public async Task<IList<T>> ExtractDataFromXlsFile(byte[] fileBytes, ExcelClassMap<T> mapper)
        {
            var records = default(List<T>);

            using (MemoryStream memStream = new MemoryStream(fileBytes))
            {
                using (var importer = new ExcelImporter(memStream))
                {
                    importer.Configuration.RegisterClassMap(mapper);
                    var sheet = importer.ReadSheet();
                    records = await Task.FromResult(sheet.ReadRows<T>().ToList());
                }
            }
            return records;
        }

        public async Task<IList<T>> ExtractDataFromXlsxFile<T>(byte[] fileBytes, T mapper) where T : class, new()
        {
            var type = mapper.GetType();
            var attibuteType = Attribute.GetCustomAttribute(type, typeof(ExcelWorksheetAttribute));
            if (attibuteType == null)
            {
                throw new ArgumentException($"{nameof(mapper)} object passed is not a type ExcelWorksheet");
            }

            var transactions = default(List<T>);

            using (var stream = new MemoryStream(fileBytes))
            {
                using (var excelPackage = new ExcelPackage(stream))
                {
                    await Task.FromResult(transactions = excelPackage.ToList<T>());
                }
            }

            return transactions;
        }
          
    }
}
