using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Models;
using FilleUploadCore.UploadManagers;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public interface IBatchProcessor
    {
        Task<ResponseResult> UploadFileAsync(FileUploadRequest httpRequest);
    }
}