using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Controllers;
using FileUploadApi.Models;
using FilleUploadCore.UploadManagers;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public interface IApiUploadService
    {
        Task<PagedData<BillPaymentRowStatus>> GetBillPaymentsStatus(string batchId, PaginationFilter pagination);

        Task<BatchFileSummaryDto> GetFileSummary(string batchId);

        Task<PagedData<BatchFileSummaryDto>> GetUserFilesSummary(string userId, PaginationFilter paginationFilter);

        Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions);
        
        Task<string> GetFileTemplateContentAsync(string extension, MemoryStream outputStream);

        Task<string> GetFileValidationResultAsync(string extension, MemoryStream outputStream);
    }

    public interface IBatchProcessor
    {
        Task<UploadResult> UploadFileAsync(HttpRequest httpRequest);
    }
}