using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace FileUploadApi
{
  
    public interface IBillPaymentDbRepository
    {
        Task<string> InsertAllUploadRecords(UploadSummaryDto fileDetail, List<BillPayment> billPayments, List<FailedBillPayment> invalidBillPayments);

        Task<BatchFileSummary> GetBatchUploadSummary(string batchId);

        Task<BillPaymentRowStatusDtoObject> GetBillPaymentRowStatuses(string batchId, PaginationFilter pagination);

        Task UpdateValidationResponse(UpdateValidationResponseModel updateBillPayments);

        Task<long> GetBatchUploadSummaryId(string batchId);

        Task<IEnumerable<ConfirmedBillPaymentDto>> GetConfirmedBillPayments(string batchId);

        Task UpdateBillPaymentInitiation(string batchId);

        Task UpdateUploadSuccess(long userId, string batchId);

        Task<IEnumerable<BatchFileSummary>> GetUploadSummariesByUserId(string userId);
    }
}