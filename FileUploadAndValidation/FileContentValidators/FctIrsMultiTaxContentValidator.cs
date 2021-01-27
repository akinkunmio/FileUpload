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
    public class FctIrsMultiTaxContentValidator : IFileContentValidator
    {
        private readonly ILogger<FctIrsMultiTaxContentValidator> _logger;

        public FctIrsMultiTaxContentValidator(ILogger<FctIrsMultiTaxContentValidator> logger)
        {
            _logger = logger;
        }
        public async Task<UploadResult> Validate(FileUploadRequest request, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrEmpty(rows, nameof(rows));

            var headerRow = new Row();
            IEnumerable<Row> contentRows = new List<Row>();
            IEnumerable<RowDetail> fctIrsPayments = new List<RowDetail>();
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

                    GenericHelpers.ValidateHeaderRow(headerRow, ContentTypeColumnContract.FirsMultiTaxWht());

                    contentRows = contentRows.Skip(1);
                }


                var validateRowsResult = await ValidateContent(request.ContentType, contentRows);

                uploadResult.Failures = validateRowsResult.Failures;
                uploadResult.ValidRows = validateRowsResult.ValidRows;

                if (uploadResult.ValidRows.Count() == 0)
                    throw new AppException("All records are invalid", 400, uploadResult);

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
                _logger.LogError($"Error occured while uploading bill payment file with error message {exception.Message} | {exception.StackTrace}", exception.Message, exception.StackTrace);
                uploadResult.ErrorMessage = exception.Message;
                throw new AppException(exception.Message, 400, uploadResult);
            }
        }

        private async Task<ValidateRowsResult> ValidateContent(string authority, IEnumerable<Row> contentRows)
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

            string rowTaxType = default;

            if (authority.ToLower().Equals(GenericConstants.Firs))
                //picks the row column(cell) that has the tax type value
                rowTaxType = row.Columns[4].Value;

            var columnContracts = GetColumnContractByTaxType(authority, rowTaxType);

            var validationResult = GenericHelpers.ValidateRowCell(row, columnContracts);

            if (rowTaxType.ToLower().Equals(GenericConstants.PreOpLevy))
                rowDetail = new RowDetail
                {
                    RowNum = row.Index,
                    ProductCode = row.Columns[0].Value,
                    ItemCode = row.Columns[1].Value,
                    CustomerId = row.Columns[2].Value,
                    Amount = row.Columns[3].Value,
                    Desc = row.Columns[4].Value,
                    CustomerName = row.Columns[5].Value,
                    PhoneNumber = row.Columns[6].Value,
                    Email = row.Columns[7].Value,
                    Address = row.Columns[8].Value
                };

            if (rowTaxType.ToLower().Equals(GenericConstants.Wht))
                rowDetail = new RowDetail
                {
                    RowNum = row.Index,
                    ProductCode = row.Columns[0].Value,
                    ItemCode = row.Columns[1].Value,
                    CustomerId = row.Columns[2].Value,
                    Amount = row.Columns[3].Value,
                    Desc = row.Columns[4].Value,
                    CustomerName = row.Columns[5].Value
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

            if (authority.ToLower().Equals(GenericConstants.FctIrs))
            {
                if (!string.IsNullOrWhiteSpace(taxType)
                   && taxType.ToLower().Equals(GenericConstants.Wht))
                    columnContracts = ContentTypeColumnContract.FctIrsMultiTaxWht();

                if (!string.IsNullOrWhiteSpace(taxType)
                   && (taxType.ToLower().Equals(GenericConstants.PreOpLevy)))
                    columnContracts = ContentTypeColumnContract.FctIrsMultiTaxPreOp();
            }

            return columnContracts;
        }
    }
}
