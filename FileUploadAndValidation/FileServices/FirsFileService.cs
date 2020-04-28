using FileUploadAndValidation;
using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.QueueMessages;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Models;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using FilleUploadCore.UploadManagers;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FileUploadAndValidation.Models.UploadResult<FileUploadAndValidation.Models.FirsRowDetail>;

namespace FileUploadApi.Services
{
    public class FirsFileService : IFileService<FirsRowDetail>
    {
        private readonly IFirsDbRepository _dbRepository;
        private readonly INasRepository _nasRepository;
        private readonly IHttpService _billPaymentService;
        private readonly IBus _bus;
        private readonly ILogger<FirsFileService> _logger;

        public FirsFileService(IFirsDbRepository dbRepository,
            INasRepository nasRepository,
            IHttpService billPaymentService,
            IBus bus,
            ILogger<FirsFileService> logger)
        {
            _dbRepository = dbRepository;
            _nasRepository = nasRepository;
            _billPaymentService = billPaymentService;
            _bus = bus;
            _logger = logger;
        }

        private async Task<ValidateRowsResult<FirsRowDetail>> ValidateContent(string validationType, IEnumerable<Row> contentRows, ColumnContract[] columnContracts)
        {
            // Console.WriteLine("Validating rows...");

            var validRows = new List<FirsRowDetail>();
          
            ValidateRowModel<FirsRowDetail> validateRowModel;
            var failures = new List<Failure<FirsRowDetail>>();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(validationType, row, columnContracts);

                if (validateRowModel.IsValid && validationType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                    validRows.Add(new FirsWhtRowDetail
                    {
                        RowNumber = row.Index,
                        BeneficiaryTin = row.Columns[0].Value,
                        BeneficiaryName = row.Columns[1].Value,
                        BeneficiaryAddress = row.Columns[2].Value,
                        ContractDate = row.Columns[3].Value,
                        ContractAmount = row.Columns[4].Value,
                        InvoiceNumber = row.Columns[5].Value,
                        ContractType = row.Columns[6].Value,
                        PeriodCovered = row.Columns[7].Value,
                        WhtRate = row.Columns[8].Value,
                        WhtAmount = row.Columns[9].Value
                    });
                else if ( validateRowModel.IsValid && validationType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                {
                    validRows.Add(new FirsWVatRowDetail
                    {
                        RowNumber = row.Index,
                        ContractorName = row.Columns[0].Value,
                        ContractorAddress = row.Columns[1].Value,
                        ContractorTin = row.Columns[2].Value,
                        ContractDescription = row.Columns[3].Value,
                        TransactionDate = row.Columns[4].Value,
                        NatureOfTransaction = row.Columns[5].Value,
                        InvoiceNumber = row.Columns[6].Value,
                        TransactionCurrency = row.Columns[7].Value,
                        CurrencyInvoicedValue = row.Columns[8].Value,
                        TransactionInvoicedValue = row.Columns[9].Value,
                        CurrencyExchangeRate = row.Columns[10].Value,
                        TaxAccountNumber = row.Columns[11].Value,
                        WvatRate = row.Columns[12].Value,
                        WvatValue = row.Columns[13].Value
                    });
                }

                if (validateRowModel.Failure != null && validateRowModel.Failure.ColumnValidationErrors != null && validateRowModel.Failure.ColumnValidationErrors.Any())
                    failures.Add(validateRowModel.Failure);
            }

            return new ValidateRowsResult<FirsRowDetail> { Failures = failures, ValidRows = validRows };
        }

        private async Task<ValidateRowModel<FirsRowDetail>> ValidateRow(string validationType, Row row, ColumnContract[] columnContracts)
        {
            var isValid = true;

            var validationErrors = GenericHelpers.ValidateRowCell(row, columnContracts, isValid);

            var failure = new Failure<FirsRowDetail>();
            var rowDetail = new FirsRowDetail();

            if (validationType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                rowDetail = new FirsWhtRowDetail
                {
                    RowNumber = row.Index,
                    BeneficiaryTin = row.Columns[0].Value,
                    BeneficiaryName = row.Columns[1].Value,
                    BeneficiaryAddress = row.Columns[2].Value,
                    ContractDate = row.Columns[3].Value,
                    ContractAmount = row.Columns[4].Value,
                    InvoiceNumber = row.Columns[5].Value,
                    ContractType = row.Columns[6].Value,
                    PeriodCovered = row.Columns[7].Value,
                    WhtRate = row.Columns[8].Value,
                    WhtAmount = row.Columns[9].Value
                };
            else if (validationType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
            {
                rowDetail = new FirsWVatRowDetail
                {
                    RowNumber = row.Index,
                    ContractorName = row.Columns[0].Value,
                    ContractorAddress = row.Columns[1].Value,
                    ContractorTin = row.Columns[2].Value,
                    ContractDescription = row.Columns[3].Value,
                    TransactionDate = row.Columns[4].Value,
                    NatureOfTransaction = row.Columns[5].Value,
                    InvoiceNumber = row.Columns[6].Value,
                    TransactionCurrency = row.Columns[7].Value,
                    CurrencyInvoicedValue = row.Columns[8].Value,
                    TransactionInvoicedValue = row.Columns[9].Value,
                    CurrencyExchangeRate = row.Columns[10].Value,
                    TaxAccountNumber = row.Columns[11].Value,
                    WvatRate = row.Columns[12].Value,
                    WvatValue = row.Columns[13].Value
                };
            }

            if (validationErrors.Count() > 0)
            {
                failure =
                    new Failure<FirsRowDetail>
                    {
                        ColumnValidationErrors = validationErrors,
                        Row = rowDetail
                    };
            }

            return await Task.FromResult(new ValidateRowModel<FirsRowDetail> { IsValid = isValid, Failure = failure });
        }

        public async Task<UploadResult<FirsRowDetail>> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows)
        {
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ContentType, nameof(uploadOptions.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ValidationType, nameof(uploadOptions.ValidationType));
            ArgumentGuard.NotNullOrEmpty(rows, nameof(rows));

            var headerRow = new Row();
            var uploadResult = new UploadResult<FirsRowDetail>();
            IEnumerable<Firs> firsPayments = new List<Firs>();
            IEnumerable<Firs> failedItemTypeValidationBills = new List<Firs>();

            try
            {
                if (!rows.Any())
                    throw new AppException("Empty file was uploaded!.");

                uploadResult.RowsCount = rows.Count() - 1;

                headerRow = rows.First();

                var columnContract = new ColumnContract[] { };

                if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                    columnContract = ContentTypeColumnContract.WHT();

                if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                    columnContract = ContentTypeColumnContract.WVAT();

                GenericHelpers.ValidateHeaderRow(headerRow, columnContract);

                var contentRows = rows.Skip(1);

                var validateRowsResult = await ValidateContent(uploadOptions.ValidationType, contentRows, columnContract);

                uploadResult.Failures = validateRowsResult.Failures;
                uploadResult.ValidRows = validateRowsResult.ValidRows;

                var dateTimeNow = DateTime.Now;

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid");

                if (uploadResult.ValidRows.Count() > 0 || uploadResult.ValidRows.Any())
                {
                    if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                    {
                        var firsWhtValidRows = (IList<FirsWhtRowDetail>)uploadResult.ValidRows;

                        firsPayments = firsWhtValidRows.Select(r => new FirsWht
                        {
                            RowNumber = r.RowNumber,
                            BeneficiaryAddress = r.BeneficiaryAddress,
                            BeneficiaryName = r.BeneficiaryName,
                            BeneficiaryTin = r.BeneficiaryTin,
                            ContractAmount = decimal.Parse(r.ContractAmount),
                            ContractDate = r.ContractDate,
                            ContractType = r.ContractType,
                            WhtRate = decimal.Parse(r.WhtRate),
                            WhtAmount = decimal.Parse(r.WhtAmount),
                            InvoiceNumber = r.InvoiceNumber,
                            PeriodCovered = r.PeriodCovered,
                            BatchId = uploadOptions.BatchId,
                            CreatedDate = dateTimeNow.ToString()
                        });
                    }
                    else if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                    {
                        var firsWVatValidRows = (IList<FirsWVatRowDetail>)uploadResult.ValidRows;

                        firsPayments = firsWVatValidRows.Select(r => new FirsWVat
                        {
                            RowNumber = r.RowNumber,
                            ContractDescription = r.ContractDescription,
                            ContractorAddress = r.ContractorAddress,
                            ContractorTin = r.ContractorTin,
                            ContractorName = r.ContractorName,
                            CurrencyInvoicedValue = decimal.Parse(r.CurrencyInvoicedValue),
                            NatureOfTransaction = r.NatureOfTransaction,
                            TaxAccountNumber = r.TaxAccountNumber,
                            TransactionInvoicedValue = decimal.Parse(r.TransactionInvoicedValue),
                            CurrencyExchangeRate = decimal.Parse(r.CurrencyExchangeRate),
                            TransactionCurrency = r.TransactionCurrency,
                            InvoiceNumber = r.InvoiceNumber,
                            TransactionDate = r.TransactionDate,
                            WvatRate = decimal.Parse(r.WvatRate),
                            WvatValue = decimal.Parse(r.WvatValue),
                            BatchId = uploadOptions.BatchId,
                            CreatedDate = dateTimeNow.ToString()
                        });
                    }

                    //var productCodeList = firsPayments.Select(s => s.ProductCode).ToArray();

                    //string firstItem = productCodeList[0];

                    //if (!string.Equals(firstItem, uploadOptions.ProductCode, StringComparison.InvariantCultureIgnoreCase))
                    //    throw new AppException($"Expected file ProductCode to be {uploadOptions.ProductCode}, but found {firstItem}!.");

                    //bool allEqual = productCodeList.Skip(1)
                    //  .All(s => string.Equals(firstItem, s, StringComparison.InvariantCultureIgnoreCase));

                    //if (!allEqual)
                    //    throw new AppException("ProductCode should have same value for all records");

                    //if (uploadOptions.ValidationType
                    //    .ToLower()
                    //    .Equals(GenericConstants.BillPaymentId.ToLower()))
                    //{
                    //    failedItemTypeValidationBills = firsPayments
                    //        ?.GroupBy(b => new { b.ProductCode, b.CustomerId })
                    //        .Where(g => g.Count() > 1)
                    //        .SelectMany(r => r);

                    //    foreach (var nonDistinct in failedItemTypeValidationBills)
                    //        uploadResult.Failures.Add(new Failure<BillPaymentRowDetail>
                    //        {
                    //            Row = new BillPaymentRowDetail
                    //            {
                    //                RowNumber = nonDistinct.RowNumber,
                    //                CustomerId = nonDistinct.CustomerId,
                    //                ItemCode = nonDistinct.ItemCode,
                    //                ProductCode = nonDistinct.ProductCode,
                    //                Amount = nonDistinct.Amount.ToString()
                    //            },
                    //            ColumnValidationErrors = new List<ValidationError>
                    //            {
                    //                new ValidationError
                    //                {
                    //                    PropertyName = "ProductCode, CustomerId",
                    //                    ErrorMessage = "Values should be unique and not be same"
                    //                }
                    //            }
                    //        });
                    //}

                    //if (uploadOptions.ValidationType
                    //    .ToLower()
                    //    .Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
                    //{
                    //    failedItemTypeValidationBills = firsPayments
                    //        ?.GroupBy(b => new { b.ItemCode, b.CustomerId })
                    //        .Where(g => g.Count() > 1)
                    //        .SelectMany(r => r);

                    //    foreach (var nonDistinct in failedItemTypeValidationBills)
                    //        uploadResult.Failures.Add(new Failure<BillPaymentRowDetail>
                    //        {
                    //            Row = new BillPaymentRowDetail
                    //            {
                    //                RowNumber = nonDistinct.RowNumber,
                    //                CustomerId = nonDistinct.CustomerId,
                    //                ItemCode = nonDistinct.ItemCode,
                    //                ProductCode = nonDistinct.ProductCode,
                    //                Amount = nonDistinct.Amount.ToString()
                    //            },
                    //            ColumnValidationErrors = new List<ValidationError>
                    //            {
                    //                new ValidationError
                    //                {
                    //                    PropertyName = "ItemCode, CustomerId",
                    //                    ErrorMessage = "Values should be unique and not be same"
                    //                }
                    //            }
                    //        });
                    //}

                    //firsPayments = firsPayments
                    //    .Where(b => !failedItemTypeValidationBills.Any(n => n.RowNumber == b.RowNumber))
                    //    .Select(r => r);

                    //uploadResult.ValidRows = firsPayments.Select(r => new BillPaymentRowDetail
                    //{
                    //    RowNumber = r.RowNumber,
                    //    Amount = r.Amount.ToString(),
                    //    ProductCode = r.ProductCode,
                    //    ItemCode = r.ItemCode,
                    //    CustomerId = r.CustomerId
                    //}).ToList();

                }


                IEnumerable<FailedFirs> failedFirs = default;
                if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                {
                    var firsWhtFailedRows = (IList<FirsWhtRowDetail>)uploadResult.Failures;

                    failedFirs = firsWhtFailedRows.Select(r => new FailedFirsWht
                    {
                        RowNumber = r.RowNumber,
                        BeneficiaryAddress = r.BeneficiaryAddress,
                        BeneficiaryName = r.BeneficiaryName,
                        BeneficiaryTin = r.BeneficiaryTin,
                        ContractAmount = r.ContractAmount,
                        ContractDate = r.ContractDate,
                        ContractType = r.ContractType,
                        WhtRate = r.WhtRate,
                        WhtAmount = r.WhtAmount,
                        InvoiceNumber = r.InvoiceNumber,
                        PeriodCovered = r.PeriodCovered,
                        BatchId = uploadOptions.BatchId,
                        CreatedDate = dateTimeNow.ToString()
                    });
                }
                else if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                {
                    var firsWVatFailedRows = (IList<FirsWVatRowDetail>)uploadResult.Failures;

                    failedFirs = firsWVatFailedRows.Select(r => new FailedFirsWVat
                    {
                        RowNumber = r.RowNumber,
                        ContractDescription = r.ContractDescription,
                        ContractorAddress = r.ContractorAddress,
                        ContractorTin = r.ContractorTin,
                        ContractorName = r.ContractorName,
                        CurrencyInvoicedValue = r.CurrencyInvoicedValue,
                        NatureOfTransaction = r.NatureOfTransaction,
                        TaxAccountNumber = r.TaxAccountNumber,
                        TransactionInvoicedValue = r.TransactionInvoicedValue,
                        CurrencyExchangeRate = r.CurrencyExchangeRate,
                        TransactionCurrency = r.TransactionCurrency,
                        InvoiceNumber = r.InvoiceNumber,
                        TransactionDate = r.TransactionDate,
                        WvatRate = r.WvatRate,
                        WvatValue = r.WvatValue,
                        BatchId = uploadOptions.BatchId,
                        CreatedDate = dateTimeNow.ToString()
                    });
                }

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
                }, firsPayments.ToList(), failedFirs.ToList(), uploadOptions.ValidationType);

                var toValidatePayments = firsPayments.Select(f =>
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
            catch (AppException appEx)
            {
                uploadResult.ErrorMessage = appEx.Message;
                appEx.Value = uploadResult;
                throw appEx;
            }
            catch (Exception exception)
            {
                _logger.LogError("Error occured while uploading bill payment file with error message {ex.message} | {ex.StackTrace}", exception.Message, exception.StackTrace);
                uploadResult.ErrorMessage = exception.Message;
                throw new AppException(exception.Message, uploadResult);
            }
        }

      
        public Task<BatchFileSummaryDto> GetBatchUploadSummary(string batchId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedData<RowStatus>> GetPaymentResults(string batchId, PaginationFilter pagination)
        {
            throw new NotImplementedException();
        }

        public Task<PagedData<BatchFileSummaryDto>> GetUserUploadSummaries(string userId, PaginationFilter paginationFilter)
        {
            throw new NotImplementedException();
        }

        public Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions)
        {
            throw new NotImplementedException();
        }

        public Task UpdateStatusFromQueue(PaymentValidateMessage queueMessage)
        {
            throw new NotImplementedException();
        }

        private string ConstructValidationError(Failure<FirsRowDetail> failure)
        {
            var result = new StringBuilder();
            foreach (var error in failure.ColumnValidationErrors)
            {
                result.Append($"{error.PropertyName}: {error.ErrorMessage}");
                result.Append(", ");
            }

            return result.ToString();
        }
    }
  
    public enum ValidationTypes
    {
        NUBAN, LUHN, BVN
    }
}

