using FileUploadAndValidation.Models;
using System.Threading.Tasks;

namespace FileUploadApi.Services
{
    public interface IFileUploadService<T> where T : class
    {
        Task<T> ProcessXlsxFile(byte[] fileBytes);

        Task<T> ProcessXlsFile(byte[] fileBytes);

        Task<T> ProcessTxtCsvFile(byte[] fileBytes);
    }
}