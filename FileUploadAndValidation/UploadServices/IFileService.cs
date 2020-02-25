﻿using FileUploadAndValidation.Models;
using FileUploadApi.Models;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileUploadAndValidation.QueueMessages;
using FileUploadAndValidation;

namespace FileUploadApi.Services
{
    public interface IFileService
    {
        Task<ValidateRowsResult> ValidateContent(IEnumerable<Row> contentRows, ColumnContract[] columnContracts);

        Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows, string batchId);

        Task<IEnumerable<BillPaymentRowStatus>> GetBillPaymentResults(string batchId, PaginationFilter pagination);

        Task<BatchFileSummaryDto> GetBatchUploadSummary(string batchId);

        Task UpdateStatusFromQueue(BillPaymentValidateMessage queueMessage);

        Task PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions);
    }
   
}