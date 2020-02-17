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

        Task<IEnumerable<BillPayment>> GetBillPayments(long id);

        Task<IEnumerable<BillPayment>> GetBillPayments(string batchId);

        Task UpdateValidationResponse(UpdateValidationResponseModel updateBillPayments);

        Task UpdateValidationResponse(string batchId, IEnumerable<RowValidationStatus> validationStatuses);
    }
}