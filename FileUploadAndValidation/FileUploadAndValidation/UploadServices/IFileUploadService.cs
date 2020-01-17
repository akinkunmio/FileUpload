using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadApi.Services
{
    public interface IFileUploadService
    {
        UploadOptions GetUploadOptions(bool validateHeaders);

        void ValidateHeader(Row headerRow);

        UploadResult ValidateContent(IEnumerable<Row> contentRows, UploadResult uploadResult);

        Task<UploadResult> Upload(IEnumerable<Row> rows, bool validateHeaders = true);
    }
}