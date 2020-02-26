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

        Task<IEnumerable<BillPaymentRowStatus>> GetBillPaymentsStatus(string batchId, PaginationFilter pagination);

        Task<BatchFileSummaryDto> GetFileSummary(string batchId);

        Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions);
    }
}