using FileUploadAndValidation;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.QueueMessages;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadAndValidation.Utils;
using FileUploadApi.Models;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using FilleUploadCore.UploadManagers;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static FileUploadAndValidation.Models.UploadResult;


namespace FileUploadApi
{
    public class BillPaymentFileService : IFileService, IFileContentValidator
    {
        private readonly IBillPaymentDbRepository _dbRepository;
        private readonly INasRepository _nasRepository;
        private readonly IBillPaymentService _billPaymentService;
        private readonly IBus _bus;
        private readonly ILogger<BillPaymentFileService> _logger;

        public BillPaymentFileService(IBillPaymentDbRepository dbRepository, 
            INasRepository nasRepository,
            IBillPaymentService billPaymentService,
            IBus bus,
            ILogger<BillPaymentFileService> logger)
        {
            _dbRepository = dbRepository;
            _nasRepository = nasRepository;
            _billPaymentService = billPaymentService;
            _bus = bus;
            _logger = logger;
        }

        public async Task<ValidateRowsResult<RowDetail>> ValidateContent(IEnumerable<Row> contentRows, ColumnContract[] columnContracts)
        {
           // Console.WriteLine("Validating rows...");

            List<BillPaymentRowDetail> validRows = new List<BillPaymentRowDetail>();
            ValidateRowModel<BillPaymentRowDetail> validateRowModel;
            var failures = new List<Failure<BillPaymentRowDetail>>();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(row, columnContracts);

                if (validateRowModel.IsValid)
                    validRows.Add( new BillPaymentRowDetail 
                    {  
                        RowNumber = row.Index, 
                        ProductCode = row.Columns[0].Value, 
                        ItemCode = row.Columns[1].Value, 
                        CustomerId = row.Columns[2].Value, 
                        Amount = row.Columns[3].Value 
                    });

                if (validateRowModel.Failure != null && validateRowModel.Failure.ColumnValidationErrors != null && validateRowModel.Failure.ColumnValidationErrors.Any())
                    failures.Add(validateRowModel.Failure);
            }

            return new ValidateRowsResult<BillPaymentRowDetail> { Failures = failures, ValidRows = validRows } ;
        }

        private async Task<ValidateRowModel<RowDetail>> ValidateRow(Row row, ColumnContract[] columnContracts)
        {
            var isValid = true;

            var validationErrors = GenericHelpers.ValidateRowCell(row, columnContracts, isValid);

            var failure = new Failure();
            
            var rowDetail = new BillPaymentRowDetail 
            {
                RowNumber = row.Index,
                ProductCode = row.Columns[0].Value,
                ItemCode = row.Columns[1].Value,
                CustomerId = row.Columns[2].Value,
                Amount = row.Columns[3].Value
            };

            if (validationErrors.Count() > 0)
            {
                failure = 
                    new Failure<BillPaymentRowDetail>
                    {
                        ColumnValidationErrors = validationErrors,
                        Row = rowDetail
                    };
            }

            return await Task.FromResult(new ValidateRowModel<BillPaymentRowDetail> { IsValid = isValid, Failure = failure });
        }

        public async Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows)
        {
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ContentType, nameof(uploadOptions.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ValidationType, nameof(uploadOptions.ValidationType));
            ArgumentGuard.NotNullOrEmpty(rows, nameof(rows));

            var headerRow = new Row();
            var uploadResult = new UploadResult();
            IEnumerable<BillPayment> billPayments = new List<BillPayment>();
            IEnumerable<BillPayment> failedItemTypeValidationBills = new List<BillPayment>();

            try
            {
                if (!rows.Any())
                    throw new AppException("Empty file was uploaded!.");

                uploadResult.RowsCount = rows.Count() - 1;

                headerRow = rows.First();

                var columnContract = new ColumnContract[] { };

                if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
                    columnContract = ContentTypeColumnContract.BillerPaymentIdWithItem();

                if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower()))
                    columnContract = ContentTypeColumnContract.BillerPaymentId();

                GenericHelpers.ValidateHeaderRow(headerRow, columnContract);

                var contentRows = rows.Skip(1);

                var validateRowsResult = await ValidateContent(contentRows, columnContract);

                uploadResult.Failures = validateRowsResult.Failures;
                uploadResult.ValidRows = validateRowsResult.ValidRows;

                var dateTimeNow = DateTime.Now;

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid");

                if (uploadResult.ValidRows.Count() > 0 || uploadResult.ValidRows.Any())
                {
                    billPayments = uploadResult.ValidRows.Select(r => new BillPayment 
                    { 
                        RowNumber = r.RowNumber,
                        Amount = double.Parse(r.Amount),
                        ProductCode = r.ProductCode,
                        ItemCode = r.ItemCode,
                        CustomerId = r.CustomerId,
                        BatchId = uploadResult.BatchId,
                        CreatedDate = dateTimeNow.ToString()
                    });

                    var productCodeList = billPayments.Select(s => s.ProductCode).ToArray();

                    string firstItem = productCodeList[0];

                    if(!string.Equals(firstItem, uploadOptions.ProductCode, StringComparison.InvariantCultureIgnoreCase))
                        throw new AppException($"Expected file ProductCode to be {uploadOptions.ProductCode}, but found {firstItem}!.");

                    bool allEqual = productCodeList.Skip(1)
                      .All(s => string.Equals(firstItem, s, StringComparison.InvariantCultureIgnoreCase));

                    if (!allEqual)
                        throw new AppException("ProductCode should have same value for all records");
                    
                    if (uploadOptions.ValidationType
                        .ToLower()
                        .Equals(GenericConstants.BillPaymentId.ToLower()))
                    {
                        failedItemTypeValidationBills = billPayments
                            ?.GroupBy(b => new { b.ProductCode, b.CustomerId })
                            .Where(g => g.Count() > 1)
                            .SelectMany(r => r);

                        foreach (var nonDistinct in failedItemTypeValidationBills)
                            uploadResult.Failures.Add(new Failure<BillPaymentRowDetail>
                            {
                                Row = new BillPaymentRowDetail
                                {
                                    RowNumber = nonDistinct.RowNumber,
                                    CustomerId = nonDistinct.CustomerId,
                                    ItemCode = nonDistinct.ItemCode,
                                    ProductCode = nonDistinct.ProductCode,
                                    Amount = nonDistinct.Amount.ToString()
                                },
                                ColumnValidationErrors = new List<ValidationError>
                                {
                                    new ValidationError
                                    {
                                        PropertyName = "ProductCode, CustomerId",
                                        ErrorMessage = "Values should be unique and not be same"
                                    }
                                }
                            }); 
                    }

                    if (uploadOptions.ValidationType
                        .ToLower()
                        .Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
                    {
                        failedItemTypeValidationBills = billPayments
                            ?.GroupBy(b => new { b.ItemCode, b.CustomerId })
                            .Where(g => g.Count() > 1)
                            .SelectMany(r => r);

                        foreach (var nonDistinct in failedItemTypeValidationBills)
                            uploadResult.Failures.Add(new Failure<BillPaymentRowDetail>
                            {
                                Row = new BillPaymentRowDetail
                                {
                                    RowNumber = nonDistinct.RowNumber,
                                    CustomerId = nonDistinct.CustomerId,
                                    ItemCode = nonDistinct.ItemCode,
                                    ProductCode = nonDistinct.ProductCode,
                                    Amount = nonDistinct.Amount.ToString()
                                },
                                ColumnValidationErrors = new List<ValidationError>
                                {
                                    new ValidationError 
                                    { 
                                        PropertyName = "ItemCode, CustomerId", 
                                        ErrorMessage = "Values should be unique and not be same" 
                                    }
                                }
                            });
                    }

                    billPayments = billPayments
                        .Where(b => !failedItemTypeValidationBills.Any(n => n.RowNumber == b.RowNumber))
                        .Select(r => r);

                    uploadResult.ValidRows = billPayments.Select(r => new BillPaymentRowDetail 
                    {
                        RowNumber = r.RowNumber,
                        Amount = r.Amount.ToString(),
                        ProductCode = r.ProductCode,
                        ItemCode = r.ItemCode,
                        CustomerId = r.CustomerId
                    }).ToList();

                }

                var failedBillPayments = uploadResult.Failures.Select(f =>
                    new FailedBillPayment
                    {
                         Amount = f.Row.Amount,
                         CreatedDate = dateTimeNow.ToString(),
                         CustomerId = f.Row.CustomerId,
                         ItemCode = f.Row.ItemCode,
                         ProductCode = f.Row.ProductCode,
                         RowNumber = f.Row.RowNumber,
                         Error = ConstructValidationError(f)
                    }
                );

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid");

                await _dbRepository.InsertAllUploadRecords(new UploadSummaryDto
                {
                    BatchId = uploadResult.BatchId,
                    NumOfAllRecords = uploadResult.RowsCount,
                    Status = GenericConstants.PendingValidation,
                    UploadDate = dateTimeNow.ToString(),
                    CustomerFileName = uploadOptions.FileName,
                    ItemType = uploadOptions.ValidationType,
                    ContentType = uploadOptions.ContentType,
                    NasRawFile = uploadOptions.RawFileLocation,
                    UserId = (long)uploadOptions.UserId
                }, billPayments.ToList(), failedBillPayments.ToList());

                var toValidatePayments = billPayments.Select(b =>
                {
                    return new NasBillPaymentDto
                    {
                        amount = b.Amount,
                        customer_id = b.CustomerId,
                        row = b.RowNumber,
                        item_code = b.ItemCode,
                        product_code = b.ProductCode,
                    };
                });
                
                FileProperty fileProperty = await _nasRepository.SaveFileToValidate(uploadResult.BatchId, toValidatePayments);

                var validationResponse = await _billPaymentService.ValidateBillRecords(fileProperty, uploadOptions.AuthToken, toValidatePayments.Count() > 50);

                string validationResultFileName;
                if (validationResponse.Data.NumOfRecords <= GenericConstants.RECORDS_SMALL_SIZE && validationResponse.Data.Results.Any() && validationResponse.Data.ResultMode.ToLower().Equals("json"))
                {
                    await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
                    {
                        BatchId = uploadResult.BatchId,
                        NasToValidateFile = fileProperty.Url,
                        ModifiedDate = DateTime.Now.ToString(),
                        NumOfValidRecords = validationResponse.Data.Results.Where(v => v.Status.ToLower().Equals("valid")).Count(),
                        Status = GenericConstants.AwaitingInitiation,
                        RowStatuses = validationResponse.Data.Results
                    });

                    var validationResult = await GetPaymentResults(uploadResult.BatchId, new PaginationFilter(uploadResult.RowsCount, 1));
                    validationResultFileName = await _nasRepository.SaveValidationResultFile(uploadResult.BatchId, validationResult.Data);

                    await _dbRepository.UpdateUploadSuccess(uploadResult.BatchId, validationResultFileName);
                }

                return uploadResult;
            }
            catch(AppException appEx)
            {
                uploadResult.ErrorMessage = appEx.Message;
                appEx.Value = uploadResult;
                throw appEx;
            }
            catch (Exception exception)
            {
                _logger.LogError("Error occured while uploading bill payment file with error message {ex.message} | {ex.StackTrace}", exception.Message, exception.StackTrace);
                uploadResult.ErrorMessage = exception.Message;
                throw new AppException(exception.Message, (int)HttpStatusCode.InternalServerError, uploadResult);
            }
        }

        private string ConstructValidationError(Failure<BillPaymentRowDetail> failure)
        {
            var result = new StringBuilder();
            foreach(var error in failure.ColumnValidationErrors)
            {
                result.Append($"{error.PropertyName}: {error.ErrorMessage}");
                result.Append(", ");
            }

            return result.ToString();
        }
       
        public async Task<PagedData<RowStatus>> GetPaymentResults(string batchId, PaginationFilter pagination)
        {
            IEnumerable<BillPayment> billPayments = new List<BillPayment>();
            IEnumerable<RowStatus> billPaymentStatuses = default;
            int totalRowCount;
            double validAmountSum;

            try
            {
                var billPaymentStatusesObj = await _dbRepository
                    .GetBillPaymentRowStatuses(batchId, pagination);

                totalRowCount = billPaymentStatusesObj.TotalRowsCount;

                validAmountSum = billPaymentStatusesObj.ValidAmountSum;

                billPaymentStatuses = billPaymentStatusesObj.RowStatusDtos.Select(s => new BillPaymentRowStatus
                 {
                      Amount = s.Amount,
                      CustomerId = s.CustomerId,
                      ItemCode = s.ItemCode,
                      ProductCode = s.ProductCode,
                      Error = s.Error,
                      Row = s.RowNum,
                      Status = s.RowStatus
                 });

                if (billPaymentStatuses.Count() < 1)
                    throw new AppException($"Upload Batch Id '{batchId}' was not found", (int)HttpStatusCode.NotFound);
            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while getting bill payment statuses with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException($"An error occured while fetching results for {batchId}!.");
            }

            return new PagedData<RowStatus> { Data = billPaymentStatuses, TotalRowsCount = totalRowCount, TotalAmountSum = validAmountSum };
        }

        public async Task<BatchFileSummaryDto> GetBatchUploadSummary(string batchId)
        {
            BatchFileSummary batchFileSummary;
            var batchFileSummaryDto = new BatchFileSummaryDto();
            try
            {
                batchFileSummary = await _dbRepository.GetBatchUploadSummary(batchId);
                
                if (batchFileSummary == null)
                    throw new AppException($"Upload with Batch Id: '{batchId}' not found!.", (int)HttpStatusCode.NotFound);

                batchFileSummaryDto = new BatchFileSummaryDto
                {
                    BatchId = batchFileSummary.BatchId,
                    ContentType = batchFileSummary.ContentType,
                    ItemType = batchFileSummary.ItemType,
                    NumOfAllRecords = batchFileSummary.NumOfRecords,
                    NumOfValidRecords = batchFileSummary.NumOfValidRecords,
                    Status = batchFileSummary.TransactionStatus,
                    UploadDate = batchFileSummary.UploadDate
                };
            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while getting upload summary with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }

            return batchFileSummaryDto;
        }

        public async Task<PagedData<BatchFileSummaryDto>> GetUserUploadSummaries(string userId, PaginationFilter paginationFilter)
        {
            var userFileSummaries = new PagedData<BatchFileSummary>();
            var pagedData = new PagedData<BatchFileSummaryDto>();
            try
            {
                userFileSummaries = await _dbRepository.GetUploadSummariesByUserId(userId, paginationFilter);
                
                pagedData.Data = userFileSummaries.Data.Select(u => new BatchFileSummaryDto
                {
                    BatchId = u.BatchId,
                    ContentType = u.ContentType,
                    ItemType = u.ItemType,
                    NumOfAllRecords = u.NumOfRecords,
                    NumOfValidRecords = u.NumOfValidRecords,
                    Status = u.TransactionStatus,
                    UploadDate = u.UploadDate,
                    FileName = GenericHelpers.GetFileNameFromBatchId(u.BatchId)
                });

                pagedData.TotalRowsCount = userFileSummaries.TotalRowsCount;

            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while getting user upload summaries with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }

            return pagedData;
        }

        public async Task UpdateStatusFromQueue(PaymentValidateMessage queueMessage)
        {
            IEnumerable<RowValidationStatus> validationStatuses; 

            try
            {
                validationStatuses = await _nasRepository.ExtractValidationResult(queueMessage);

                if (validationStatuses.Count() > 0 || !validationStatuses.Any())
                    await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
                    {
                        BatchId = queueMessage.BatchId,
                        ModifiedDate = DateTime.Now.ToString(),
                        NasToValidateFile = queueMessage.ResultLocation,
                        NumOfValidRecords = validationStatuses.Where(v => v.Status.ToLower().Equals("valid")).Count(),
                        Status = GenericConstants.AwaitingInitiation,
                        RowStatuses = validationStatuses.ToList()
                    });                
                throw new AppException($"File to be validated with batch Id:'{queueMessage.BatchId}' has no content on NAS!");

            }
            catch(AppException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while updating queue status with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
        }

        public async Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions)
        {
            ConfirmedBillResponse result;
            try
            {
                var confirmedBillPayments = await _dbRepository.GetConfirmedBillPayments(batchId);

                if (confirmedBillPayments.Count() < 0 || !confirmedBillPayments.Any())
                    throw new AppException($"Records awaiting payment initiation not found for batch Id: {batchId}", (int)HttpStatusCode.NotFound);

                var nasDto = confirmedBillPayments
                    .Select(e =>
                        new NasBillPaymentDto
                        {
                            amount = e.Amount,
                            customer_id = e.CustomerId,
                            item_code = e.ItemCode,
                            product_code = e.ProductCode,
                            row = e.RowNum
                        });

                var fileProperty = await _nasRepository.SaveFileToConfirmed(batchId, nasDto);
                result = await _billPaymentService.ConfirmedBillRecords(fileProperty, initiatePaymentOptions);

                await _dbRepository.UpdateBillPaymentInitiation(batchId);
            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch(Exception ex)
            {
                _logger.LogError("Error occured while while initiating payment with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException("An error occured while initiating payment");
            }

            return result;
        }
    }

    public class ValidateRowModel<T>
    {
        public bool IsValid { get; set; }
        public Failure<T> Failure { get; set; }
    }
    public class ValidateRowsResult<T>
    {
        public List<Failure<T>> Failures { get; set; }
        public List<T> ValidRows { get; set; }
    }
}