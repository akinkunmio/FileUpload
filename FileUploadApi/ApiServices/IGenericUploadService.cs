using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Models;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public interface IGenericUploadService
    {
        Task<PagedData<BatchFileSummaryDto>> GetUserFilesSummary(string userId, SummaryPaginationFilter paginationFilter);

        Task<PagedData<dynamic>> GetPaymentsStatus(string batchId, PaginationFilter pagination, string auth);

        Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions);

        Task<FileValidationResultModel> GetFileValidationResultAsync(string batchId, MemoryStream outputStream);

        Task<string> GetFileTemplateContentAsync(string contentType, string itemType, MemoryStream outputStream);
    }
}
