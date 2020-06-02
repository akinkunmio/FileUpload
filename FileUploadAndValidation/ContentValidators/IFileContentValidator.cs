using FileUploadAndValidation;
using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadApi.Services
{
    public interface IFileContentValidator<T, TContext> where T : ValidatedRow
    {
        Task<ValidationResult<T>> Validate(IEnumerable<Row> rows, TContext context);
        bool CanProcess(string contentType);
    }
}