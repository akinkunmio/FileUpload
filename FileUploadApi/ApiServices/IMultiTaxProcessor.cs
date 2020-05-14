using FileUploadAndValidation.Models;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public interface IMultiTaxProcessor
    {
        Task<ResponseResult> UploadFileAsync(FileUploadRequest uploadRequest);
    }
}