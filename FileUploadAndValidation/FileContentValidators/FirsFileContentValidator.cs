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
        private async Task<ValidateRowsResult> ValidateContent(string itemType, IEnumerable<Row> contentRows, ColumnContract[] columnContracts)
        {
            var validRows = new List<RowDetail>();

            ValidateRowModel validateRowModel;
            var failures = new List<Failure>();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(itemType, row, columnContracts);

                if (validateRowModel.isValid)
                    validRows.Add(validateRowModel.Valid);

                if (!validateRowModel.isValid)
                    failures.Add(validateRowModel.Failure);
            }

            return new ValidateRowsResult { Failures = failures, ValidRows = validRows };
        }

        private async Task<ValidateRowModel> ValidateRow(string itemType, Row row, ColumnContract[] columnContracts)
        {
            var rowDetail = new RowDetail();
            var result = new ValidateRowModel();

            var validationResult = GenericHelpers.ValidateRowCell(row, columnContracts);

            if (itemType.ToLower().Equals(GenericConstants.Wht.ToLower()))
                rowDetail = new RowDetail
                {
                    RowNum = row.Index,
                    BeneficiaryTin = row.Columns[0].Value,
                    BeneficiaryName = row.Columns[1].Value,
                    BeneficiaryAddress = row.Columns[2].Value,
                    ContractDate = row.Columns[3].Value,
                    ContractDescription = row.Columns[4].Value,
                    ContractAmount = row.Columns[5].Value,
                    ContractType = row.Columns[6].Value,
                    PeriodCovered = row.Columns[7].Value,
                    InvoiceNumber = row.Columns[8].Value,
                    WhtRate = row.Columns[9].Value,
                    WhtAmount = row.Columns[10].Value
                };
            else if (itemType.ToLower().Equals(GenericConstants.Wvat.ToLower()))
            {
                rowDetail = new RowDetail
                {
                    RowNum = row.Index,
                    ContractorName = row.Columns[0].Value,
                    ContractorAddress = row.Columns[1].Value,
                    ContractorTin = row.Columns[2].Value,
                    ContractDescription = row.Columns[3].Value,
                    NatureOfTransaction = row.Columns[4].Value,
                    TransactionDate = row.Columns[5].Value,
                    InvoiceNumber = row.Columns[6].Value,
                    TransactionCurrency = row.Columns[7].Value,
                    CurrencyInvoicedValue = row.Columns[8].Value,
                    CurrencyExchangeRate = row.Columns[9].Value,
                    TransactionInvoicedValue = row.Columns[10].Value,
                    WvatRate = row.Columns[11].Value,
                    WvatValue = row.Columns[12].Value,
                    TaxAccountNumber = row.Columns[13].Value,
                };
            }

            result.isValid = validationResult.Validity;

            if (validationResult.Validity)
            {
                result.Valid = rowDetail;
            }
            else
            {
                result.Failure = new Failure
                {
                    ColumnValidationErrors = validationResult.ValidationErrors,
                    Row = rowDetail
                };
            }

            return await Task.FromResult(result);
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
                    throw new AppException("Empty file was uploaded!.", 400);

                var columnContract = new ColumnContract[] { };

                if (request.ItemType.ToLower().Equals(GenericConstants.Wht.ToLower()))
                    columnContract = ContentTypeColumnContract.FirsWht();

                if (request.ItemType.ToLower().Equals(GenericConstants.Wvat.ToLower()))
                    columnContract = ContentTypeColumnContract.FirsWvat();

                uploadResult.RowsCount = rows.Count();
                var contentRows = rows;

                if (request.HasHeaderRow)
                {
                    uploadResult.RowsCount = rows.Count() - 1;

                    headerRow = rows.First();

                    GenericHelpers.ValidateHeaderRow(headerRow, columnContract);

                    contentRows = rows.Skip(1);
                }

                var validateRowsResult = await ValidateContent(request.ItemType, contentRows, columnContract);

                uploadResult.Failures = validateRowsResult.Failures;
                uploadResult.ValidRows = validateRowsResult.ValidRows;

                var dateTimeNow = DateTime.Now;

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid", 400, uploadResult);

                if (uploadResult.ValidRows.Count() > 0 
                    && uploadResult.ValidRows.Any() 
                    && request.ItemType.ToLower().Equals(GenericConstants.Wht))
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
                                RowNum = nonDistinct.RowNum,
                                BeneficiaryAddress = nonDistinct.BeneficiaryAddress,
                                BeneficiaryName = nonDistinct.BeneficiaryName,
                                BeneficiaryTin = nonDistinct.BeneficiaryTin,
                                ContractDescription = nonDistinct.ContractDescription,
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

                if (uploadResult.ValidRows.Count() > 0 
                    && uploadResult.ValidRows.Any() 
                    && request.ItemType.ToLower().Equals(GenericConstants.Wvat))
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
                                RowNum = nonDistinct.RowNum,
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
                        .Where(b => !failedItemTypeValidationBills.Any(n => n.RowNum == b.RowNum))
                        .Select(r => r).ToList();

                foreach(var failure in uploadResult.Failures)
                {
                    failure.Row.Error = GenericHelpers.ConstructValidationError(failure);
                }

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid", 400, uploadResult);

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
