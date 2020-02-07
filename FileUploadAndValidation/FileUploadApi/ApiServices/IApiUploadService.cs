using FileUploadAndValidation.Models;
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

        Task<IEnumerable<BillPaymentStatus>> GetBillPaymentsStatus(string scheduleId, string userName);

        Task<BatchFileSummaryDto> GetBatchFileSummary(string scheduleId, string userName);
    }
}