using Microsoft.AspNetCore.Http;

namespace FileUploadAndValidation.Models
{
    public interface IFileUploadRequest
    {
        string AuthToken { get; set; }
        string ContentType { get; set; }
        string FileExtension { get; set; }
        string FileName { get; set; }
        IFormFile FileRef { get; }
        long FileSize { get; set; }
        string ItemType { get; set; }
        string ProductCode { get; set; }
        string RawFileLocation { get; set; }
        long? UserId { get; set; }
    }
}