using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace FileUploadApi
{
  
    public interface IBillPaymentDbRepository
    {
        Task<string> InsertPaymentUpload(UploadSummaryDto fileDetail, List<BillPayment> billPayments);

        Task<BatchFileSummary> GetBatchUploadSummary(string batchId);

        Task<IEnumerable<BillPaymentRowStatus>> GetBillPaymentRowStatuses(string batchId);

        Task UpdateValidationResponse(UpdateValidationResponseModel updateBillPayments);

        Task<long> GetBatchUploadSummaryId(string batchId);

        Task<IEnumerable<ConfirmedBillPaymentDto>> GetConfirmedBillPayments(string batchId);
    }
}