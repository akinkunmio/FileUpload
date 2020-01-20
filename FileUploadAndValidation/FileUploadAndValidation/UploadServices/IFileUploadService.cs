using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadApi.Services
{
    public interface IFileUploadService
    {
        Task<UploadResult> Upload(IEnumerable<Row> rows, bool validateHeaders = true);
    }
}