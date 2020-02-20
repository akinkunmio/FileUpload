using FileUploadAndValidation.Models;
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

        Task<IEnumerable<BillPaymentRowStatus>> GetBillPaymentsStatus(string batchId);

        Task<BatchFileSummaryDto> GetFileSummary(string batchId);

        Task PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions);
    }
}