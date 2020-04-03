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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static FileUploadAndValidation.Models.UploadResult;


namespace FileUploadApi
{
    public class BulkBillPaymentFileService : IFileService
    {
        private readonly IBillPaymentDbRepository _dbRepository;
        private readonly INasRepository _nasRepository;
        private readonly IBillPaymentService _billPaymentService;
        private readonly IBus _bus;

        public BulkBillPaymentFileService(IBillPaymentDbRepository dbRepository, 
            INasRepository nasRepository,
            IBillPaymentService billPaymentService,
            IBus bus)
        {
            _dbRepository = dbRepository;
            _nasRepository = nasRepository;
            _billPaymentService = billPaymentService;
            _bus = bus;
        }

        public async Task<ValidateRowsResult> ValidateContent(IEnumerable<Row> contentRows, ColumnContract[] columnContracts)
        {
           // Console.WriteLine("Validating rows...");

            List<RowDetail> validRows = new List<RowDetail>();
            ValidateRowModel validateRowModel;
            var failures = new List<Failure>();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(row, columnContracts);

                if (validateRowModel.IsValid)
                    validRows.Add( new RowDetail 
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

            return new ValidateRowsResult { Failures = failures, ValidRows = validRows } ;
        }

        private async Task<ValidateRowModel> ValidateRow(Row row, ColumnContract[] columnContracts)
        {
            var validationErrors = new List<ValidationError>();
            var failure = new Failure();
            var rowDetail = new RowDetail 
            {
                RowNumber = row.Index,
                ProductCode = row.Columns[0].Value,
                ItemCode = row.Columns[1].Value,
                CustomerId = row.Columns[2].Value,
                Amount = row.Columns[3].Value
            };

            var isValid = true;

            for (var i = 0; i < columnContracts.Length; i++)
            {
                var errorMessage = "";
                ColumnContract contract = columnContracts[i];
                Column column = row.Columns[i];
                

                try
                {
                    if (contract.Required == true && string.IsNullOrWhiteSpace(column.Value))
                    {
                        errorMessage = "Value must be provided";
                    }
                    if (contract.Max != default && column.Value != null && contract.Max < column.Value.Length)
                    {
                        errorMessage = "Specified maximum length exceeded";
                    }
                    if (contract.Min != default && column.Value != null && column.Value.Length < contract.Min)
                    {
                        errorMessage = "Specified minimum length not met";
                    }
                    if (contract.DataType != default && column.Value != null)
                    {
                        if (!GenericHelpers.ColumnDataTypes().ContainsKey(contract.DataType))
                            errorMessage = "Specified data type is not supported";
                        try
                        {
                            dynamic typedValue = Convert.ChangeType(column.Value, GenericHelpers.ColumnDataTypes()[contract.DataType]);
                        }
                        catch (Exception)
                        {
                            errorMessage = "Invalid value for data type specified";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                        throw new ValidationException(
                            new ValidationError
                            {
                                ErrorMessage = errorMessage,
                                PropertyName = contract.ColumnName
                            },
                            errorMessage);
                }
                catch (ValidationException exception)
                {
                    isValid = false;
                    validationErrors.Add(exception.ValidationError);
                }
            }

            if (validationErrors.Count() > 0)
            {
                failure = 
                    new Failure
                    {
                        ColumnValidationErrors = validationErrors,
                        Row = rowDetail
                    };
            }

            return await Task.FromResult(new ValidateRowModel { IsValid = isValid, Failure = failure });
        }

        public void ValidateHeaderRow(Row headerRow, ColumnContract[] columnContracts)
        {
            if (headerRow == null)
                throw new ValidationException("Header row not found");

            var expectedNumOfColumns = columnContracts.Count();
            if (headerRow.Columns.Count() != expectedNumOfColumns)
                throw new ValidationException($"Invalid number of header columns. Expected: {expectedNumOfColumns}, Found: {headerRow.Columns.Count()}");

            for (int i = 0; i < expectedNumOfColumns; i++)
            {
                var columnName = columnContracts[i].ColumnName;
                var headerRowColumn = headerRow.Columns[i].Value.ToString().Trim();
                if (!headerRowColumn.ToLower().Contains(columnName.ToLower()))
                    throw new ValidationException($"Invalid header column name. Expected: {columnName}, Found: {headerRowColumn}");
            }
        }

        public async Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ContentType, nameof(uploadOptions.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ItemType, nameof(uploadOptions.ItemType));
            ArgumentGuard.NotNullOrEmpty(rows, nameof(rows));

            var headerRow = new Row();
            IEnumerable<BillPayment> billPayments = new List<BillPayment>();
            IEnumerable<BillPayment> nonDistincts = new List<BillPayment>();
           
            try
            {
                if (!rows.Any())
                    throw new AppException("Empty file was uploaded!.");
                
                headerRow = rows.First();

                var columnContract = new ColumnContract[] { };

                if (uploadOptions.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
                    columnContract = ContentTypeColumnContract.BillerPaymentIdWithItem();

                if (uploadOptions.ItemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower()))
                    columnContract = ContentTypeColumnContract.BillerPaymentId();

                ValidateHeaderRow(headerRow, columnContract);

                var contentRows = rows.Skip(1);

                var validateRowsResult = await ValidateContent(contentRows, columnContract);

                uploadResult.Failures = validateRowsResult.Failures;
                uploadResult.ValidRows = validateRowsResult.ValidRows;

                var dateTimeNow = DateTime.Now;

                if (uploadResult.ValidRows.Count() > 0 || uploadResult.ValidRows.Any())
                {
                    billPayments = uploadResult.ValidRows.Select(r => new BillPayment 
                    { 
                        RowNumber = r.RowNumber,
                        Amount = Convert.ToDouble(r.Amount),
                        ProductCode = r.ProductCode,
                        ItemCode = r.ItemCode,
                        CustomerId = r.CustomerId,
                        BatchId = uploadResult.BatchId,
                        CreatedDate = dateTimeNow.ToString()
                    });

                    if(uploadOptions.ItemType
                        .ToLower()
                        .Equals(GenericConstants.BillPaymentId.ToLower()))
                    {
                        nonDistincts = billPayments
                            ?.GroupBy(b => new { b.ProductCode, b.CustomerId })
                            .Where(g => g.Count() > 1)
                            .SelectMany(r => r);

                        foreach (var nonDistinct in nonDistincts)
                            uploadResult.Failures.Add(new Failure
                            {
                                Row = new RowDetail
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
                                        ErrorMessage = "Values should be unique and not be repeated"
                                    }
                                }
                            }); 
                    }

                    if (uploadOptions.ItemType
                        .ToLower()
                        .Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
                    {
                        nonDistincts = billPayments
                            ?.GroupBy(b => new { b.ItemCode, b.CustomerId })
                            .Where(g => g.Count() > 1)
                            .SelectMany(r => r);

                        foreach (var nonDistinct in nonDistincts)
                            uploadResult.Failures.Add(new Failure
                            {
                                Row = new RowDetail
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
                                        ErrorMessage = "Values should be unique and not be repeated" 
                                    }
                                }
                            });
                    }

                    billPayments = billPayments
                        .Where(b => !nonDistincts.Any(n => n.RowNumber == b.RowNumber))
                        .Select(r => r);

                    uploadResult.ValidRows = billPayments.Select(r => new RowDetail 
                    {
                        RowNumber = r.RowNumber,
                        Amount = r.Amount.ToString(),
                        ProductCode = r.ProductCode,
                        ItemCode = r.ItemCode,
                        CustomerId = r.CustomerId
                    }).ToList();

                    uploadResult.RowsCount = uploadResult.ValidRows.Count();
                }

                await _dbRepository.InsertPaymentUpload(
                    new UploadSummaryDto
                    {
                        BatchId = uploadResult.BatchId,
                        NumOfAllRecords = uploadResult.RowsCount,
                        Status = GenericConstants.PendingValidation,
                        UploadDate = dateTimeNow.ToString(),
                        CustomerFileName = uploadOptions.FileName,
                        ItemType = uploadOptions.ItemType,
                        ContentType = uploadOptions.ContentType,
                        NasRawFile = uploadOptions.RawFileLocation,
                    }, billPayments.ToList());

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

                if (validationResponse.Data.NumOfRecords <= GenericConstants.RECORDS_SMALL_SIZE && validationResponse.Data.Results.Any() && validationResponse.Data.ResultMode.ToLower().Equals("json"))
                    await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
                    {
                        BatchId = uploadResult.BatchId,
                        NasToValidateFile = fileProperty.Url,
                        ModifiedDate = DateTime.Now.ToString(),
                        NumOfValidRecords = validationResponse.Data.Results.Where(v => v.Status.ToLower().Equals("valid")).Count(),
                        Status = GenericConstants.AwaitingInitiation,
                        RowStatuses = validationResponse.Data.Results
                    });
                
                await _dbRepository.UpdateUploadSuccess((long)uploadOptions.UserId, uploadResult.BatchId);

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
                uploadResult.ErrorMessage = exception.Message;
                throw new AppException(exception.Message, (int)HttpStatusCode.InternalServerError, uploadResult);
            }
        }

        public async Task<BillPaymentRowStatusObject> GetBillPaymentResults(string batchId, PaginationFilter pagination)
        {
            IEnumerable<BillPayment> billPayments = new List<BillPayment>();
            IEnumerable<BillPaymentRowStatus> billPaymentStatuses = default;
            int totalRowCount;

            try
            {
                var billPaymentStatusesObj = await _dbRepository
                    .GetBillPaymentRowStatuses(batchId, pagination);

                totalRowCount = billPaymentStatusesObj.TotalRowsCount;

                billPaymentStatuses = billPaymentStatusesObj.RowStatusDtos.Select(s => new BillPaymentRowStatus
                 {
                      Error = s.Error,
                      Row = s.RowNum,
                      Status = s.RowStatus
                 });

                if (billPaymentStatuses.Count() < 0)
                    throw new AppException($"Upload Batch Id:{batchId} was not found", (int)HttpStatusCode.NotFound);


            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception)
            {
                throw new AppException($"An error occured while fetching results for {batchId}!.");
            }

            return new BillPaymentRowStatusObject { RowStatuses = billPaymentStatuses, TotalRowsCount = totalRowCount };
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
                throw ex;
            }

            return batchFileSummaryDto;
        }

        public async Task UpdateStatusFromQueue(BillPaymentValidateMessage queueMessage)
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
            catch(Exception)
            {
                throw new AppException("An error occured while initiating payment");
            }

            return result;
        }
    }

    public class ValidateRowModel
    {
        public bool IsValid { get; set; }
        public Failure Failure { get; set; }
    }
    public class ValidateRowsResult
    {
        public List<Failure> Failures { get; set; }
        public List<RowDetail> ValidRows { get; set; }
    }
}