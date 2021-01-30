using FileUploadAndValidation.FileServices;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.FileContentValidators
{
    public class FirsSingleTaxContentValidator : IFileContentValidator
    {
        private readonly ILogger<FirsSingleTaxContentValidator> _logger;
        public FirsSingleTaxContentValidator(ILogger<FirsSingleTaxContentValidator> logger)
        {
            _logger = logger;
        }

        private async Task<ValidateRowsResult> ValidateContent(string authority, IEnumerable<Row> contentRows, ColumnContract[] columnContracts)
        {
            var validRows = new List<RowDetail>();

            ValidateRowModel validateRowModel;
            var failures = new List<Failure>();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(authority, row);

                if (validateRowModel.IsValid)
                    validRows.Add(validateRowModel.ValidRow);

                if (!validateRowModel.IsValid)
                    failures.Add(validateRowModel.Failure);
            }

            return new ValidateRowsResult { Failures = failures, ValidRows = validRows };
        }

       
        private ColumnContract[] GetColumnContractByTaxType(string authority, string taxType)
        {
            ColumnContract[] columnContracts = default;

            if (authority.ToLower().Equals(GenericConstants.Firs))
            {
                if (!string.IsNullOrWhiteSpace(taxType)
                   && taxType.ToLower().Equals(GenericConstants.Wht))
                    columnContracts = ContentTypeColumnContract.FirsWht();

                else if (!string.IsNullOrWhiteSpace(taxType)
                   && taxType.ToLower().Equals(GenericConstants.Wvat))
                    columnContracts = ContentTypeColumnContract.FirsWvat();
                else
                    columnContracts = ContentTypeColumnContract.FirsTaxOther();
            }

            return columnContracts;
        }

        public async Task<UploadResult> Validate(FileUploadRequest request, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrEmpty(rows, nameof(rows));

            var headerRow = new Row();
            IEnumerable<Row> contentRows = new List<Row>();
            IEnumerable<RowDetail> failDistinctValidation = new List<RowDetail>();
            var columnContract = new ColumnContract[] { };

            try
            {
                if (!rows.Any())
                    throw new AppException("Empty file was uploaded!.", 400);

                uploadResult.RowsCount = rows.Count();
                contentRows = rows;

                if (request.HasHeaderRow)
                {
                    uploadResult.RowsCount -= 1;

                    headerRow = rows.First();

                    GenericHelpers.ValidateHeaderRow(headerRow, ContentTypeColumnContract.FirsWht());

                    contentRows = contentRows.Skip(1);
                }


                var validateRowsResult = await ValidateContent(request.ContentType, contentRows, columnContract);

                uploadResult.Failures = validateRowsResult.Failures;
                uploadResult.ValidRows = validateRowsResult.ValidRows;

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid", 400, uploadResult);

                if (uploadResult.ValidRows.Any())
                {
                    var itemType = uploadResult.ValidRows.FirstOrDefault().TaxType;
                    if (itemType.ToLower().Equals(GenericConstants.Wht))
                    {
                        var whtRowDetails = uploadResult.ValidRows
                          .Where(u => GenericConstants.Wht.Equals(u.TaxType, StringComparison.InvariantCultureIgnoreCase))
                          .Select(v => v);

                        failDistinctValidation = whtRowDetails
                                  ?.GroupBy(b => new { b?.BeneficiaryTin })
                                  .Where(g => g.Count() > 1)
                                  .SelectMany(r => r);

                        foreach (var nonDistinct in failDistinctValidation)
                            uploadResult.Failures.Add(new Failure
                            {
                                Row = nonDistinct,
                                ColumnValidationErrors = new List<ValidationError>
                                {
                                    new ValidationError
                                    {
                                        PropertyName = "Beneficiary Tin",
                                        ErrorMessage = "Value should be unique for wht tax type"
                                    }
                                }
                            });
                    }
                    if (itemType.ToLower().Equals(GenericConstants.Wvat))
                    {
                        var wvatRowDetails = uploadResult.ValidRows
                              .Where(u => GenericConstants.Wvat.Equals(u.TaxType, StringComparison.InvariantCultureIgnoreCase))
                              .Select(v => v);

                        failDistinctValidation = wvatRowDetails
                                  ?.GroupBy(b => new { b?.ContractorTin })
                                  .Where(g => g.Count() > 1)
                                  .SelectMany(r => r);

                        foreach (var nonDistinct in failDistinctValidation)
                            uploadResult.Failures.Add(new Failure
                            {
                                Row = nonDistinct,
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
                };


                uploadResult.ValidRows = uploadResult.ValidRows?.Except(failDistinctValidation).ToList();

                if (uploadResult.Failures.Any())
                    foreach (var failure in uploadResult.Failures)
                    {
                        failure.Row.ErrorDescription = GenericHelpers.ConstructValidationError(failure);
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
                _logger.LogError($"Error occured while uploading firs multitax payment file with error message {exception.Message} | {exception.StackTrace}", exception.Message, exception.StackTrace);
                uploadResult.ErrorMessage = exception.Message;
                throw new AppException(exception.Message, 400, uploadResult);
            }
        }
        private async Task<ValidateRowModel> ValidateRow(string authority, Row row)
        {
            var rowDetail = new RowDetail();
            var result = new ValidateRowModel();

            string rowTaxType = "";

            if (authority.ToLower().Equals(GenericConstants.Firs))
                rowTaxType = row.Columns[27].Value;

            if (string.IsNullOrWhiteSpace(rowTaxType))
                return new ValidateRowModel
                {
                    IsValid = false,
                    Failure = new Failure
                    {
                        ColumnValidationErrors = new List<ValidationError> {
                             new ValidationError {
                                ErrorMessage = $"Field should not be empty",
                                PropertyName = ContentTypeColumnContract.FirsWht()[27].ColumnName
                             }
                         },
                        Row = new RowDetail
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
                            WVATRate = row.Columns[11].Value,
                            WVATValue = row.Columns[12].Value,
                            TaxAccountNumber = row.Columns[13].Value,
                            BeneficiaryTin = row.Columns[14].Value,
                            BeneficiaryName = row.Columns[15].Value,
                            BeneficiaryAddress = row.Columns[16].Value,
                            ContractDate = row.Columns[17].Value,
                            ContractAmount = row.Columns[18].Value,
                            ContractType = row.Columns[19].Value,
                            PeriodCovered = row.Columns[20].Value,
                            WhtRate = row.Columns[21].Value,
                            WhtAmount = row.Columns[22].Value,
                            Amount = row.Columns[23].Value,
                            Comment = row.Columns[24].Value,
                            DocumentNumber = row.Columns[25].Value,
                            CustomerTin = row.Columns[26].Value,
                            TaxType = row.Columns[27].Value
                        }
                    }

                };

            var columnContracts = GetColumnContractByTaxType(authority, rowTaxType);

            ValidateRowResult validationResult = GenericHelpers.ValidateRowCell(row, columnContracts);

            if (rowTaxType.ToLower().Equals(GenericConstants.Wvat))
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
                    WVATRate = row.Columns[11].Value,
                    WVATValue = row.Columns[12].Value,
                    TaxAccountNumber = row.Columns[13].Value,
                    TaxType = row.Columns[27].Value
                };
            else if (rowTaxType.ToLower().Equals(GenericConstants.Wht))
                rowDetail = new RowDetail
                {
                    RowNum = row.Index,
                    BeneficiaryTin = row.Columns[14].Value,
                    BeneficiaryName = row.Columns[15].Value,
                    BeneficiaryAddress = row.Columns[16].Value,
                    ContractDate = row.Columns[17].Value,
                    ContractDescription = row.Columns[3].Value,
                    ContractAmount = row.Columns[18].Value,
                    ContractType = row.Columns[19].Value,
                    PeriodCovered = row.Columns[20].Value,
                    InvoiceNumber = row.Columns[6].Value,
                    WhtRate = row.Columns[21].Value,
                    WhtAmount = row.Columns[22].Value,
                    TaxType = row.Columns[27].Value
                };
              else
                rowDetail = new RowDetail
                {
                    RowNum = row.Index,
                    Amount = row.Columns[23].Value,
                    Comment = row.Columns[24].Value,
                    DocumentNumber = row.Columns[25].Value,
                    CustomerTin = row.Columns[26].Value,
                    TaxType = row.Columns[27].Value
                };

            result.IsValid = validationResult.IsValid;

            if (validationResult.IsValid)
            {
                result.ValidRow = rowDetail;
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
    }
}
