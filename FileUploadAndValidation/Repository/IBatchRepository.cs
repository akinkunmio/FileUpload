using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Repository
{
    public class BatchRepository : IBatchRepository
    {
        private readonly IDbRepository<BillPayment, FailedBillPayment> _dbRepository;

        public BatchRepository(IDbRepository<BillPayment, FailedBillPayment> dbRepository)
        {
            _dbRepository = dbRepository;
        }

        public async Task Save(string batchId, IFileUploadRequest request, IList<RowDetail> validRows, IList<Failure> failures)
        {
            await Task.CompletedTask;
            //var totalNoOfRows = validRows.Count + failures.Count;
            //var billPaymentValidRows = 

            //await _dbRepository.InsertAllUploadRecords(new UploadSummaryDto
            //{
            //    BatchId = batchId,
            //    NumOfAllRecords = totalNoOfRows,
            //    Status = GenericConstants.PendingValidation,
            //    UploadDate = DateTime.Now.ToString(),
            //    CustomerFileName = request.FileName,
            //    ItemType = request.ItemType,
            //    ContentType = request.ContentType,
            //    NasRawFile = request.RawFileLocation,
            //    UserId = (long)request.UserId
            //}, validRows, failures);

            //var toValidatePayments = billPayments.Select(b =>
            //{
            //    return new NasBillPaymentDto
            //    {
            //        amount = b.Amount,
            //        customer_id = b.CustomerId,
            //        row = b.RowNumber,
            //        item_code = b.ItemCode,
            //        product_code = b.ProductCode,
            //    };
            //});

            //FileProperty fileProperty = await _nasRepository.SaveFileToValidate(uploadResult.BatchId, toValidatePayments);

            //var validationResponse = await _billPaymentService.ValidateBillRecords(fileProperty, uploadOptions.AuthToken, toValidatePayments.Count() > 50);

            //string validationResultFileName;
            //if (validationResponse.Data.NumOfRecords <= GenericConstants.RECORDS_SMALL_SIZE && validationResponse.Data.Results.Any() && validationResponse.Data.ResultMode.ToLower().Equals("json"))
            //{
            //    await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
            //    {
            //        BatchId = uploadResult.BatchId,
            //        NasToValidateFile = fileProperty.Url,
            //        ModifiedDate = DateTime.Now.ToString(),
            //        NumOfValidRecords = validationResponse.Data.Results.Where(v => v.Status.ToLower().Equals("valid")).Count(),
            //        Status = GenericConstants.AwaitingInitiation,
            //        RowStatuses = validationResponse.Data.Results
            //    });

            //    var validationResult = await GetPaymentResults(uploadResult.BatchId, new PaginationFilter(uploadResult.RowsCount, 1));
            //    validationResultFileName = await _nasRepository.SaveValidationResultFile(uploadResult.BatchId, validationResult.Data);

            //    await _dbRepository.UpdateUploadSuccess(uploadResult.BatchId, validationResultFileName);
            //}
        }

       
    }
    public interface IBatchRepository
    {
        Task Save(string batchId, IFileUploadRequest request, IList<RowDetail> validRows, IList<Failure> failures);
    }

}
