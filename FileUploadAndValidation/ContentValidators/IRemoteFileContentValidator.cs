using System.Collections.Generic;
using System.Threading.Tasks;
using FileUploadAndValidation;

namespace FileUploadApi.Services
{
    public interface IRemoteFileContentValidator<T> where T : ValidatedRow
    {
        Task<ValidationResult<T>> Validate(string requestIdentifier, IEnumerable<T> validRows);
    }
}