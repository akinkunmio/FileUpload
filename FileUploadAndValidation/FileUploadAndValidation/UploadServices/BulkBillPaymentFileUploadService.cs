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
using QueueServiceBus.BusProviders;
using QueueServiceBus.MessageBus;
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
        private readonly IMessageBus _bus;

        public BulkBillPaymentFileService(IBillPaymentDbRepository dbRepository, 
            INasRepository nasRepository, IBillPaymentService billPaymentService,
            IMessageBus bus)
        {
            _dbRepository = dbRepository;
            _nasRepository = nasRepository;
            _billPaymentService = billPaymentService;
            _bus = bus;
        }

        public async Task<ValidateRowsResult> ValidateContent(IEnumerable<Row> contentRows, ColumnContract[] columnContracts)
        {
            Console.WriteLine("Validating rows...");

            List<int> validRows = new List<int>();
            var validateRowModel = new ValidateRowModel();
            var failures = new List<Failure>();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(row, columnContracts);

                if (validateRowModel.IsValid)
                    validRows.Add(row.Index);

                if (validateRowModel.Failure.ColumnValidationErrors != null && validateRowModel.Failure.RowNumber != null)
                    failures.Add(validateRowModel.Failure);
            }

            return new ValidateRowsResult { Failures = failures, ValidRows = validRows } ;
        }

        private async Task<ValidateRowModel> ValidateRow(Row row, ColumnContract[] columnContracts)
        {
            var validationErrors = new List<ValidationError>();
            var failure = new Failure();

            var errorMessage = "";
            var isValid = true;

            for (var i = 0; i < columnContracts.Length; i++)
            {
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
                        RowNumber = row.Index
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
            IEnumerable<BillPayment> nonDistinct = new List<BillPayment>();
            uploadResult.RowsCount = rows.Count();

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
                

                var contentRows = uploadOptions.ValidateHeaders ? rows.Skip(1) : rows;

                var validateRowsResult = await ValidateContent(contentRows, columnContract);

                uploadResult.Failures = validateRowsResult.Failures;
                uploadResult.ValidRows = validateRowsResult.ValidRows;

                var dateTimeNow = DateTime.Now;

                if (uploadResult.ValidRows.Count() > 0 || uploadResult.ValidRows.Any())
                {
                    billPayments = rows
                        .Where(row => uploadResult.ValidRows.Contains(row.Index))
                        .Select(r => new BillPayment
                        {
                            RowNumber = r.Index,
                            ProductCode = r.Columns[0].Value.ToString(),
                            ItemCode = r.Columns[1].Value.ToString(),
                            CustomerId = r.Columns[2].Value.ToString(),
                            Amount = Convert.ToInt32(r.Columns[3].Value),
                            BatchId = uploadResult.BatchId,
                            CreatedDate = dateTimeNow.ToString()
                        });


                    if(uploadOptions.ItemType
                        .ToLower()
                        .Equals(GenericConstants.BillPaymentId.ToLower()))
                    {
                        nonDistinct = billPayments
                            .GroupBy(b => new { b.ProductCode, b.CustomerId })
                            .Where(g => g.Count() > 1)
                            .SelectMany(r => r);

                        foreach (var nonDist in nonDistinct)
                            uploadResult.Failures.Add(new Failure 
                            {
                                RowNumber = nonDist.RowNumber,
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
                        nonDistinct = billPayments
                            .GroupBy(b => new { b.ItemCode, b.CustomerId })
                            .Where(g => g.Count() > 1)
                            .SelectMany(r => r);

                        foreach (var nonDist in nonDistinct)
                            uploadResult.Failures.Add(new Failure
                            {
                                RowNumber = nonDist.RowNumber,
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
                        .Where(b => !nonDistinct.Any(n => n.RowNumber == b.RowNumber))
                        .Select(r => r);
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
                        NasRawFile = uploadOptions.NasFileLocation,
                    }, billPayments.ToList());

                var toValidatePayments = billPayments.Select(b =>
                {
                    return new NasBillPaymentDto
                    {
                        Amount = b.Amount,
                        CustomerId = b.CustomerId,
                        Row = b.RowNumber,
                        ItemCode = b.ItemCode,
                        ProductCode = b.ProductCode,
                    };
                });
                
                FileProperty fileProperty = await _nasRepository.SaveFileToValidate(uploadResult.BatchId, toValidatePayments);

                ValidationResponse validationResponse = await _billPaymentService.ValidateBillRecords(fileProperty, uploadOptions.AuthToken);

                if(validationResponse.Results
                    .Where(v => v.Status.ToLower().Equals("valid")).Count() < 50 
                    && validationResponse.Results.Any() && validationResponse.Results.Count > 0)
                {
                    await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel 
                    { 
                         BatchId = uploadResult.BatchId,
                         NasToValidateFile = fileProperty.Url,
                         ModifiedDate = DateTime.Now.ToString(),
                         NumOfValidRecords = validationResponse.Results.Where(v => v.Status.ToLower().Equals("valid")).Count(),
                         Status = GenericConstants.AwaitingInitiation,
                         RowStatuses = validationResponse.Results
                    });
                }
                else
                {
                    // send file location to queue
                    await _bus.PublishAsync(new BillPaymentConfirmedMessage(fileProperty.Url, uploadResult.BatchId, DateTime.Now));
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
                uploadResult.ErrorMessage = exception.Message;
                return uploadResult;
            }
        }

        public async Task<IEnumerable<BillPaymentRowStatus>> GetBillPaymentResults(string batchId)
        {
            IEnumerable<BillPayment> billPayments = new List<BillPayment>();
            IEnumerable<BillPaymentRowStatus> billPaymentStatuses = default;

            try
            {
                billPaymentStatuses = await _dbRepository
                    .GetBillPaymentRowStatuses(batchId);

                if (billPaymentStatuses.Count() < 0)
                    throw new AppException($"Upload with Batch Id:{batchId} was not found", (int)HttpStatusCode.NotFound);
            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception)
            {
                throw new AppException($"An error occured while fetching results for {batchId}!.");
            }
            return billPaymentStatuses;
        }

        public async Task<BatchFileSummaryDto> GetBatchUploadSummary(string batchId)
        {
            BatchFileSummary batchFileSummary;
            var batchFileSummaryDto = new BatchFileSummaryDto();
            try
            {
                batchFileSummary = await _dbRepository.GetBatchUploadSummary(batchId);
                batchFileSummaryDto = new BatchFileSummaryDto
                {
                    BatchId = batchFileSummary.BatchId,
                    ContentType = batchFileSummary.ContentType,
                    ItemType = batchFileSummary.ItemType,
                    NumOfAllRecords = batchFileSummary.NumOfValidRecords,
                    NumOfValidRecords = batchFileSummary.NumOfValidRecords,
                    Status = batchFileSummary.Status,
                    UploadDate = batchFileSummary.UploadDate
                };
            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception)
            {
                throw;
            }

            return batchFileSummaryDto;
        }

        public async Task UpdateStatusFromQueue(BillPaymentValidateMessage queueMessage)
        {
            IEnumerable<RowValidationStatus> validationStatuses; 

            try
            {
                validationStatuses = await _nasRepository.ExtractValidationResult(queueMessage);

                if (validationStatuses.Count() > 0)
                    await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
                    {
                        BatchId = queueMessage.BatchId,
                        ModifiedDate = DateTime.Now.ToString(),
                        NasToValidateFile = queueMessage.ResultLocation,
                        NumOfValidRecords = validationStatuses.Where(v => v.Status.ToLower().Equals("valid")).Count(),
                        Status = GenericConstants.AwaitingInitiation,
                        RowStatuses = validationStatuses.ToList()
                    });
                else
                    throw new AppException($"File to be validated with batch Id:{queueMessage.BatchId} has no content on NAS!");

            }
            catch (Exception ex)
            {
                throw ex;
            }
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
        public List<int> ValidRows { get; set; }
    }
}