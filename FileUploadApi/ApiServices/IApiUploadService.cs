using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Controllers;
using FileUploadApi.Models;
using FilleUploadCore.UploadManagers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public interface IApiUploadService
    {
        Task<UploadResult> UploadFileAsync(UploadOptions uploadOptions, Stream stream);

        Task<BillPaymentRowStatusObject> GetBillPaymentsStatus(string batchId, PaginationFilter pagination);

        Task<BatchFileSummaryDto> GetFileSummary(string batchId);

        Task<List<BatchFileSummaryDto>> GetUserFilesSummary(string userId);

        Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions);
        
        Task<string> GetFileTemplateContentAsync(string extension, MemoryStream outputStream);

        Task<string> GetFileValidationResultAsync(string extension, MemoryStream outputStream);

    }
}