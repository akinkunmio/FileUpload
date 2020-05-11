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
        Task<UploadResult> UploadFileAsync(FileUploadRequest httpRequest);

        Task<PagedData<BatchFileSummaryDto>> GetUserFilesSummary(string userId, PaginationFilter paginationFilter);

        Task<PagedData<dynamic>> GetBillPaymentsStatus(string batchId, PaginationFilter pagination);

        Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions);

        Task<FileValidationResultModel> GetFileValidationResultAsync(string batchId, MemoryStream outputStream);

        Task<FileTemplateModel> GetFileTemplateContentAsync(string contentType, string itemType, MemoryStream outputStream);
    }
}