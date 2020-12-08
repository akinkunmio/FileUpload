﻿using FileUploadAndValidation.FileServices;
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
    public class FirsMultiTaxContentValidator : IFileContentValidator
    {
        private readonly ILogger<FirsMultiTaxContentValidator> _logger;

        public FirsMultiTaxContentValidator(ILogger<FirsMultiTaxContentValidator> logger)
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

        private async Task<ValidateRowModel> ValidateRow(string authority, Row row)
        {
            var rowDetail = new RowDetail();
            var result = new ValidateRowModel();

            string rowTaxType = "";

            if(authority.ToLower().Equals(GenericConstants.Firs))
                rowTaxType = row.Columns[15].Value;

            if (string.IsNullOrWhiteSpace(rowTaxType))
                return new ValidateRowModel
                {
                    IsValid = false,
                    Failure = new Failure
                    {
                        ColumnValidationErrors = new List<ValidationError> {
                             new ValidationError {
                                ErrorMessage = $"Field should not be empty",
                                PropertyName = ContentTypeColumnContract.FirsMultiTaxWht()[15].ColumnName
                             }
                         },
                        Row = new RowDetail
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
                            WhtAmount = row.Columns[10].Value,
                            Amount = row.Columns[11].Value,
                            Comment = row.Columns[12].Value,
                            DocumentNumber = row.Columns[13].Value,
                            PayerTin = row.Columns[14].Value,
                            TaxType = row.Columns[15].Value
                        }
                    }

                };
                
            var columnContracts = GetColumnContractByTaxType(authority, rowTaxType);
              
            ValidateRowResult validationResult = GenericHelpers.ValidateRowCell(row, columnContracts);

            if (rowTaxType.ToLower().Equals(GenericConstants.Wht))
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
                    WhtAmount = row.Columns[10].Value,
                    PayerTin = row.Columns[14].Value,
                    TaxType = row.Columns[15].Value
                };

            if (rowTaxType.ToLower().Equals(GenericConstants.Vat)
                || rowTaxType.ToLower().Equals(GenericConstants.Cit)
                || rowTaxType.ToLower().Equals(GenericConstants.Edt)
                || rowTaxType.ToLower().Equals(GenericConstants.PreOpLevy))
                rowDetail = new RowDetail
                {
                    RowNum = row.Index,
                    Amount = row.Columns[11].Value,
                    Comment = row.Columns[12].Value,
                    DocumentNumber = row.Columns[13].Value,
                    PayerTin = row.Columns[14].Value,
                    TaxType = row.Columns[15].Value
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

        private ColumnContract[] GetColumnContractByTaxType(string authority, string taxType)
        {
            ColumnContract[] columnContracts = default;

            if (authority.ToLower().Equals(GenericConstants.Firs))
            {
                if (!string.IsNullOrWhiteSpace(taxType)
                   && taxType.ToLower().Equals(GenericConstants.Wht))
                    columnContracts = ContentTypeColumnContract.FirsMultiTaxWht();

                if (!string.IsNullOrWhiteSpace(taxType)
                   && (taxType.ToLower().Equals(GenericConstants.Vat)
                   || taxType.ToLower().Equals(GenericConstants.PreOpLevy)
                   || taxType.ToLower().Equals(GenericConstants.Cit)
                   || taxType.ToLower().Equals(GenericConstants.Edt)))
                    columnContracts = ContentTypeColumnContract.FirsMultiTaxOther();
            }

            return columnContracts;
        }
        
        public async Task<UploadResult> Validate(FileUploadRequest request, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrEmpty(rows, nameof(rows));

            var headerRow = new Row();
            IEnumerable<Row> contentRows = new List<Row>();
            IEnumerable<RowDetail> firsPayments = new List<RowDetail>();
            IEnumerable<RowDetail> failBeneficiaryTinValidation = new List<RowDetail>();
            IEnumerable<RowDetail> failPayerTinValidation = new List<RowDetail>();
            var columnContract = new ColumnContract[] { };

            try
            {
                if (!rows.Any())
                    throw new AppException("Empty file was uploaded!.",400);

                uploadResult.RowsCount = rows.Count();
                contentRows = rows;

                if (request.HasHeaderRow)
                {
                    uploadResult.RowsCount -= 1;

                    headerRow = rows.First();

                    GenericHelpers.ValidateHeaderRow(headerRow, ContentTypeColumnContract.FirsMultiTaxWht());

                    contentRows = contentRows.Skip(1);
                }


                var validateRowsResult = await ValidateContent(request.ContentType, contentRows, columnContract);

                uploadResult.Failures = validateRowsResult.Failures;
                uploadResult.ValidRows = validateRowsResult.ValidRows;

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid", 400, uploadResult);

                if (uploadResult.ValidRows.Any())
                {

                    var whtRowDetails = uploadResult.ValidRows
                              .Where(u => GenericConstants.Wht.Equals(u.TaxType, StringComparison.InvariantCultureIgnoreCase))
                              .Select(v => v);

                    failBeneficiaryTinValidation = whtRowDetails
                              ?.GroupBy(b => new { b?.BeneficiaryTin })
                              .Where(g => g.Count() > 1)
                              .SelectMany(r => r);

                    foreach (var nonDistinct in failBeneficiaryTinValidation)
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
                };


               uploadResult.ValidRows = uploadResult.ValidRows?.Except(failBeneficiaryTinValidation).ToList();

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
                _logger.LogError("Error occured while uploading firs multitax payment file with error message {ex.message} | {ex.StackTrace}", exception.Message, exception.StackTrace);
                uploadResult.ErrorMessage = exception.Message;
                throw new AppException(exception.Message, 400, uploadResult);
            }
        }
    }
}
