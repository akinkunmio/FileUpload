using FileUploadAndValidation.DTOs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileUploadAndValidation.FileDataExtractor
{
    public interface IDataExtractor<T> where T : class
    {
        Task<IList<T>> ExtractDataFromXlxsFile(byte[] fileBytes);

        Task<IList<T>> ExtractDataFromXlsFile(byte[] fileBytes);

        Task<IList<T>> ExtractDataFromTxtCsvFile(byte[] fileBytes);

    }
}