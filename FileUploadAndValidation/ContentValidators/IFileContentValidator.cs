using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadApi.Services
{
    public interface IFileContentValidator<T>
    {
        Task<ValidationResult<T>> Validate(IEnumerable<Row> rows);
        Task<ValidationResult<T>> ValidateRemote(IEnumerable<T> rows);
        bool CanProcess(string contentType);
    }
}