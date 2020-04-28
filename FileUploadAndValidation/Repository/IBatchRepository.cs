using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Repository
{
    public class BillPaymentBatchRepository : IBatchRepository
    {
        private readonly IHttpService _httpService;
        private readonly INasRepository _nasRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IDbRepository<BillPaymentRowStatusDto> _dbRepository;



        public BillPaymentBatchRepository(IBatchRepository batchRepository,
            IDbRepository<BillPaymentRowStatusDto> dbRepository,
            INasRepository nasRepository,
            IHttpService httpService, IBill)
        {
            _batchRepository = batchRepository;
            _dbRepository = dbRepository;
            _nasRepository = nasRepository;
            _httpService = httpService;
        }

        public async Task Save(string batchId, FileUploadRequest request, IList<RowDetail> validRows, IList<Failure> failures)
        {
            var totalNoOfRows = validRows.Count + failures.Count;
            //var billPaymentValidRows =
            //var failedBillPayments = failures.Select(f =>
            //        new FailedBillPayment
            //        {
            //            Amount = f.Row.Amount,
            //            CreatedDate = DateTime.Now.ToString(),
            //            CustomerId = f.Row.CustomerId,
            //            ItemCode = f.Row.ItemCode,
            //            ProductCode = f.Row.ProductCode,
            //            RowNumber = f.Row.RowNumber,
            //            Error = ConstructValidationError(f)
            //        }
            //    );
            await _dbRepository.InsertAllUploadRecords(new UploadSummaryDto
            {
                BatchId = batchId,
                NumOfAllRecords = totalNoOfRows,
                Status = GenericConstants.PendingValidation,
                UploadDate = DateTime.Now.ToString(),
                CustomerFileName = request.FileName,
                ItemType = request.ItemType,
                ContentType = request.ContentType,
                NasRawFile = request.RawFileLocation,
                UserId = (long)request.UserId
            }, validRows, failures);

            var recordsForEntValidation = validRows.Select(b =>
            {
                return new NasBillPaymentDto
                {
                    amount = double.Parse(b.Amount),
                    customer_id = b.CustomerId,
                    row = b.RowNumber,
                    item_code = b.ItemCode,
                    product_code = b.ProductCode,
                };
            });

            FileProperty fileProperty = await _nasRepository.SaveFileToValidate(batchId, recordsForEntValidation);

            var validationResponse = await _httpService.ValidateBillRecords(fileProperty, request.AuthToken, recordsForEntValidation.Count() > 50);

            string validationResultFileName;

            if (validationResponse.Data.NumOfRecords <= GenericConstants.RECORDS_SMALL_SIZE && validationResponse.Data.Results.Any() && validationResponse.Data.ResultMode.ToLower().Equals("json"))
            {
                var entValidatedRecordsCount = validationResponse.Data.Results.Where(v => v.Status.ToLower().Equals("valid")).Count();
                
                await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
                {
                    BatchId = batchId,
                    NasToValidateFile = fileProperty.Url,
                    ModifiedDate = DateTime.Now.ToString(),
                    NumOfValidRecords = entValidatedRecordsCount,
                    Status = (entValidatedRecordsCount > 0) ? GenericConstants.AwaitingInitiation : GenericConstants.NoValidRecord,
                    RowStatuses = validationResponse.Data.Results
                });

                var validationResult = await GetPaymentResults(batchId, new PaginationFilter(totalNoOfRows, 1));
                validationResultFileName = await _nasRepository.SaveValidationResultFile(uploadResult.BatchId, validationResult.Data);

                await _dbRepository.UpdateUploadSuccess(uploadResult.BatchId, validationResultFileName);
            }
        }


        private string ConstructValidationError(Failure failure)
        {
            var result = new StringBuilder();
            for (int i = 0; i < failure.ColumnValidationErrors.Count(); i++)
            {
                result.Append($"{failure.ColumnValidationErrors[i].PropertyName}: {failure.ColumnValidationErrors[i].ErrorMessage}");

                if (failure.ColumnValidationErrors[i + 1] != null)
                    result.Append(", ");
            }

            return result.ToString();
        }

    }
    public interface IBatchRepository
    {
        Task Save(string batchId, FileUploadRequest request, IList<RowDetail> validRows, IList<Failure> failures);
    }

}
