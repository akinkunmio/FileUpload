using ExcelMapper;
using FileUploadAndValidation.DTOs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

namespace FileUploadAndValidation.FileDataExtractor
{
    public interface IDataExtractor<T> where T : class
    {
        Task<IList<T>> ExtractDataFromXlsxFile<T>(byte[] fileBytes, T mapper) where T : class, new();

        Task<IList<T>> ExtractDataFromXlsFile(byte[] fileBytes, ExcelClassMap<T> mapper);

        Task<IList<T>> ExtractDataFromTxtCsvFile(byte[] fileBytes, ICsvMapping<T> csvMapping);

    }
}