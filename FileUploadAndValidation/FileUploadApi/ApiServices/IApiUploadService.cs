using FileUploadAndValidation.Models;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public interface IApiUploadService
    {
        Task<UploadResult> UploadFileAsync(FileTypes fileExtension, byte[] content);
    }
}