using FileUploadAndValidation.Models;
using FileUploadApi.Models;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadApi.Services
{
    public interface IFileService
    {
        Task<ValidateRowsResult> ValidateContent(IEnumerable<Row> contentRows);

        Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows, UploadResult uploadResult);

        Task<IEnumerable<BillPaymentRowStatus>> GetBillPaymentResults(string batchId);

        Task<BatchFileSummaryDto> GetBatchUploadSummary(string batchId);

        Task<Task> UpdateStatusFromQueueStatus(BillPaymentValidatedQueueMessage queueMessage);
    }
   
}