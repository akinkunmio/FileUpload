﻿using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace FileUploadApi
{
  
    public interface IDbRepository
    {
        Task<string> InsertAllUploadRecords(UploadSummaryDto fileDetail, IList<RowDetail> billPayments, IList<Failure> invalidBillPayments);

        Task<BatchFileSummary> GetBatchUploadSummary(string batchId);

        Task<IEnumerable<RowDetail>> GetPaymentRowStatuses(string batchId, PaginationFilter pagination);

        Task UpdateValidationResponse(UpdateValidationResponseModel updateBillPayments);

        Task<long> GetBatchUploadSummaryId(string batchId);

        Task<IEnumerable<RowDetail>> GetConfirmedPayments(string batchId);

        Task UpdatePaymentInitiation(string batchId);

        Task UpdateUploadSuccess(string batchId, string userValidationFileName);

        Task<PagedData<BatchFileSummary>> GetUploadSummariesByUserId(string userId, SummaryPaginationFilter paginationFilter);
    }
}