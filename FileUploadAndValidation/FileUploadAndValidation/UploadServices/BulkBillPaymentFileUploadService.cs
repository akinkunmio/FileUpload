using FileUploadAndValidation;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadAndValidation.Utils;
using FileUploadApi.Models;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public BulkBillPaymentFileService(IBillPaymentDbRepository dbRepository, INasRepository nasRepository, IBillPaymentService billPaymentService)
        {
            _dbRepository = dbRepository;
            _nasRepository = nasRepository;
            _billPaymentService = billPaymentService;
        }

        public async Task<ValidateRowsResult> ValidateContent(IEnumerable<Row> contentRows)
        {
            Console.WriteLine("Validating rows...");

            List<int> validRows = new List<int>();
            var validateRowModel = new ValidateRowModel();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(row);

                if (validateRowModel.IsValid)
                    validRows.Add(row.Index);
            }

            return new ValidateRowsResult { Failures = validateRowModel.Failures, ValidRows = validRows } ;
        }

        private async Task<ValidateRowModel> ValidateRow(Row row)
        {
            var validationErrors = new List<ValidationError>();
            var failures = new List<Failure>();

            var errorMessage = "";
            var isValid = true;

            for (var i = 0; i < ContentTypeColumnContract.BillerPayment().Length; i++)
            {
                ColumnContract contract = ContentTypeColumnContract.BillerPayment()[i];
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
                failures.Add(
                    new Failure
                    {
                        ColumnValidationErrors = validationErrors,
                        RowNumber = row.Index
                    });
            }

            return new ValidateRowModel { IsValid = isValid, Failures = failures };
        }

        public async Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ContentType, nameof(uploadOptions.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ItemType, nameof(uploadOptions.ItemType));
            ArgumentGuard.NotNullOrEmpty(rows, nameof(rows));

            var headerRow = new Row();
            IEnumerable<BillPayment> billPayments = new List<BillPayment>();
            uploadResult.RowsCount = rows.Count();
            try
            {
                if (!rows.Any())
                    throw new AppException("Empty file was uploaded!.");

                if (uploadOptions.ValidateHeaders)
                {
                    headerRow = rows.First();
                    GenericHelpers.ValidateHeaderRow(headerRow, ContentTypeColumnContract.BillerPayment());
                }

                var contentRows = uploadOptions.ValidateHeaders ? rows.Skip(1) : rows;

                var validateRowsResult = await ValidateContent(contentRows);

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

                if(validationResponse.NumOfRecords < 50 && validationResponse.Results.Any() && validationResponse.Results.Count > 0)
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

                return uploadResult;
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
                billPayments = await _dbRepository
                    .GetBillPayments(batchId);

                billPaymentStatuses = billPayments
                    .Select(p => new BillPaymentRowStatus
                    {
                        Error = p.Error,
                        Row = p.RowNumber,
                        Status = p.Status,
                    });
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

        public async Task<Task> UpdateStatusFromQueueStatus(BillPaymentValidatedQueueMessage queueMessage)
        {
            IEnumerable<RowValidationStatus> validationStatuses; 
            try
            {
                validationStatuses = await _nasRepository.ExtractValidationResult(queueMessage);

                if(validationStatuses.Count() > 0)
                {
                    await _dbRepository.UpdateValidationResponse(queueMessage.BatchId, validationStatuses);

                }
            }
            catch (Exception)
            {
                throw;
            }
            return Task.CompletedTask;
        }

    }

    public class ValidateRowModel
    {
        public bool IsValid { get; set; }
        public List<Failure> Failures { get; set; }
    }
    public class ValidateRowsResult
    {
        public List<Failure> Failures { get; set; }
        public List<int> ValidRows { get; set; }
    }
}