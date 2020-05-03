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
        Task<UploadResult> UploadFileAsync(HttpRequest httpRequest);

        Task<PagedData<BatchFileSummaryDto>> GetUserFilesSummary(string userId, PaginationFilter paginationFilter);

        Task<PagedData<dynamic>> GetBillPaymentsStatus(string batchId, PaginationFilter pagination);

        Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions);

        Task<string> GetFileValidationResultAsync(string batchId, MemoryStream outputStream);

        Task<string> GetFileTemplateContentAsync(string itemType, MemoryStream outputStream);
    }
}