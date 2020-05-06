using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadApi;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.FileServices
{
    public class FirsFileContentValidator : IFileContentValidator
    {
        private readonly ILogger<FirsFileContentValidator> _logger;

        public FirsFileContentValidator(ILogger<FirsFileContentValidator> logger)
        {
            _logger = logger;
        }
        private async Task<ValidateRowsResult> ValidateContent(string validationType, IEnumerable<Row> contentRows, ColumnContract[] columnContracts)
        {
            // Console.WriteLine("Validating rows...");

            var validRows = new List<RowDetail>();

            ValidateRowModel validateRowModel;
            var failures = new List<Failure>();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(validationType, row, columnContracts);

                if (validateRowModel.IsValid && validationType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                    validRows.Add(new RowDetail
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
                else if (validateRowModel.IsValid && validationType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                {
                    validRows.Add(new RowDetail
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

            return new ValidateRowsResult { Failures = failures, ValidRows = validRows };
        }

        private async Task<ValidateRowModel> ValidateRow(string validationType, Row row, ColumnContract[] columnContracts)
        {
            var isValid = true;

            var validationErrors = GenericHelpers.ValidateRowCell(row, columnContracts, isValid);

            var failure = new Failure();
            var rowDetail = new RowDetail();

            if (validationType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                rowDetail = new RowDetail
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
                rowDetail = new RowDetail
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
                    new Failure
                    {
                        ColumnValidationErrors = validationErrors,
                        Row = rowDetail
                    };
            }

            return await Task.FromResult(new ValidateRowModel { IsValid = isValid, Failure = failure });
        }

        public async Task<UploadResult> Validate(FileUploadRequest request, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrEmpty(rows, nameof(rows));

            var headerRow = new Row();
            IEnumerable<RowDetail> firsPayments = new List<RowDetail>();
            IEnumerable<RowDetail> failedItemTypeValidationBills = new List<RowDetail>();

            try
            {
                if (!rows.Any())
                    throw new AppException("Empty file was uploaded!.");

                uploadResult.RowsCount = rows.Count() - 1;

                headerRow = rows.First();

                var columnContract = new ColumnContract[] { };

                if (request.ItemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                    columnContract = ContentTypeColumnContract.WHT();

                if (request.ItemType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                    columnContract = ContentTypeColumnContract.WVAT();

                GenericHelpers.ValidateHeaderRow(headerRow, columnContract);

                var contentRows = rows.Skip(1);

                var validateRowsResult = await ValidateContent(request.ItemType, contentRows, columnContract);

                uploadResult.Failures = validateRowsResult.Failures;
                uploadResult.ValidRows = validateRowsResult.ValidRows;

                var dateTimeNow = DateTime.Now;

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid");

                if (uploadResult.ValidRows.Count() > 0 || uploadResult.ValidRows.Any() || request.ItemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                {
                    
                    failedItemTypeValidationBills = uploadResult.ValidRows
                          ?.GroupBy(b => new { b.BeneficiaryTin })
                          .Where(g => g.Count() > 1)
                          .SelectMany(r => r);

                    foreach (var nonDistinct in failedItemTypeValidationBills)
                        uploadResult.Failures.Add(new Failure
                        {
                            Row = new RowDetail
                            {
                                RowNumber = nonDistinct.RowNumber,
                                BeneficiaryAddress = nonDistinct.BeneficiaryAddress,
                                BeneficiaryName = nonDistinct.BeneficiaryName,
                                BeneficiaryTin = nonDistinct.BeneficiaryTin,
                                ContractAmount = nonDistinct.ContractAmount,
                                ContractDate = nonDistinct.ContractDate,
                                ContractType = nonDistinct.ContractType,
                                WhtRate = nonDistinct.WhtRate,
                                WhtAmount = nonDistinct.WhtAmount,
                                InvoiceNumber = nonDistinct.InvoiceNumber,
                                PeriodCovered = nonDistinct.PeriodCovered,
                                CreatedDate = dateTimeNow.ToString()
                            },
                            ColumnValidationErrors = new List<ValidationError>
                                {
                                    new ValidationError
                                    {
                                        PropertyName = "Beneficiary Tin",
                                        ErrorMessage = "Values should be unique and not be same"
                                    }
                                }
                        });
                }

                if (uploadResult.ValidRows.Count() > 0 || uploadResult.ValidRows.Any() || request.ItemType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                {
                    failedItemTypeValidationBills = uploadResult.ValidRows
                         ?.GroupBy(b => new { b.ContractorTin })
                         .Where(g => g.Count() > 1)
                         .SelectMany(r => r);

                    foreach (var nonDistinct in failedItemTypeValidationBills)
                        uploadResult.Failures.Add(new Failure
                        {
                            Row = new RowDetail
                            {
                                RowNumber = nonDistinct.RowNumber,
                                ContractDescription = nonDistinct.ContractDescription,
                                ContractorAddress = nonDistinct.ContractorAddress,
                                ContractorTin = nonDistinct.ContractorTin,
                                ContractorName = nonDistinct.ContractorName,
                                CurrencyInvoicedValue = nonDistinct.CurrencyInvoicedValue,
                                NatureOfTransaction = nonDistinct.NatureOfTransaction,
                                TaxAccountNumber = nonDistinct.TaxAccountNumber,
                                TransactionInvoicedValue = nonDistinct.TransactionInvoicedValue,
                                CurrencyExchangeRate = nonDistinct.CurrencyExchangeRate,
                                TransactionCurrency = nonDistinct.TransactionCurrency,
                                InvoiceNumber = nonDistinct.InvoiceNumber,
                                TransactionDate = nonDistinct.TransactionDate,
                                WvatRate = nonDistinct.WvatRate,
                                WvatValue = nonDistinct.WvatValue,
                                CreatedDate = dateTimeNow.ToString()
                            },
                            ColumnValidationErrors = new List<ValidationError>
                                {
                                    new ValidationError
                                    {
                                        PropertyName = "Contractor Tin",
                                        ErrorMessage = "Values should be unique"
                                    }
                                }
                        });
                }

                uploadResult.ValidRows = uploadResult.ValidRows
                        .Where(b => !failedItemTypeValidationBills.Any(n => n.RowNumber == b.RowNumber))
                        .Select(r => r).ToList();

                foreach(var failure in uploadResult.Failures)
                {
                    failure.Row.Error = GenericHelpers.ConstructValidationError(failure);
                }

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid");

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
                throw new AppException(exception.Message, 400, uploadResult);
            }
        }
    } 
}
