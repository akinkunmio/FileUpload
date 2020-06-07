using FileUploadAndValidation.Models;
using FileUploadApi.Models;
using FilleUploadCore.UploadManagers;
using System;
using System.Threading.Tasks;
using FileUploadAndValidation.QueueMessages;
using FileUploadAndValidation;
using FileUploadAndValidation.UploadServices;

namespace FileUploadApi.Services
{
    public interface IFileService
    {
        Task<PagedData<dynamic>> GetPaymentResults(string batchId, PaginationFilter pagination);

        Task<BatchFileSummaryDto> GetBatchUploadSummary(string batchId);

        Task UpdateStatusFromQueue(PaymentValidateMessage queueMessage);

        Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions);

        Task<PagedData<BatchFileSummaryDto>> GetUserUploadSummaries(string userId, PaginationFilter paginationFilter);
    }

    

}