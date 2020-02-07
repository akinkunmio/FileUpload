using FileUploadAndValidation.Models;
using FileUploadApi.Models;
using FilleUploadCore.UploadManagers;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public interface IApiUploadService
    {
        Task<UploadResult> UploadFileAsync(UploadOptions uploadOptions, FileTypes fileExtension, byte[] content);
    }
}