using FileUploadAndValidation.Models;
using FileUploadApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Repository
{
    public interface IFirsDbRepository
    {
        Task<string> InsertAllUploadRecords(UploadSummaryDto fileDetail, List<Firs> billPayments, List<FailedFirs> invalidBillPayments, string validationType);

        Task<BatchFileSummary> GetBatchUploadSummary(string batchId);

        Task<BillPaymentRowStatusDtoObject> GetBillPaymentRowStatuses(string batchId, PaginationFilter pagination);

        Task UpdateValidationResponse(UpdateValidationResponseModel updateBillPayments);

        Task<long> GetBatchUploadSummaryId(string batchId);

        Task<IEnumerable<ConfirmedBillPaymentDto>> GetConfirmedBillPayments(string batchId);

        Task UpdateBillPaymentInitiation(string batchId);

        Task UpdateUploadSuccess(string batchId, string userValidationFileName);

        Task<PagedData<BatchFileSummary>> GetUploadSummariesByUserId(string userId, PaginationFilter paginationFilter);
    }
}