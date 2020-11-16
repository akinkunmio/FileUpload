using System.Collections.Generic;
using System.Threading.Tasks;
using FileUploadAndValidation;

namespace FileUploadApi.Services
{
    public interface IRemoteFileContentValidator<T> where T : ValidatedRow
    {
        bool IsBackground();

        Task<ValidationResult<T>> Validate(string requestIdentifier, IEnumerable<T> validRows, long businessId = 0, string clientToken ="");
    }
}