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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.FileServices
{
    public class BillPaymentContentValidator : IFileContentValidator
    {
        private readonly ILogger<BillPaymentContentValidator> _logger;
        public BillPaymentContentValidator(ILogger<BillPaymentContentValidator> logger)
        {
            _logger = logger;
        }

        public async Task<UploadResult> Validate(FileUploadRequest request, IEnumerable<Row> rows)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
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

                if (request.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
                    columnContract = ContentTypeColumnContract.BillerPaymentIdWithItem();

                if (request.ItemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower()))
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

                    if (!string.Equals(firstItem, request.ProductCode, StringComparison.InvariantCultureIgnoreCase))
                        throw new AppException($"Expected file ProductCode to be {request.ProductCode}, but found {firstItem}!.");

                    bool allEqual = productCodeList.Skip(1)
                      .All(s => string.Equals(firstItem, s, StringComparison.InvariantCultureIgnoreCase));

                    if (!allEqual)
                        throw new AppException("ProductCode should have same value for all records");

                    if (request.ItemType
                        .ToLower()
                        .Equals(GenericConstants.BillPaymentId.ToLower()))
                    {
                        failedItemTypeValidationBills = billPayments
                            ?.GroupBy(b => new { b.CustomerId })
                            .Where(g => g.Count() > 1)
                            .SelectMany(r => r);

                        foreach (var nonDistinct in failedItemTypeValidationBills)
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
                                        PropertyName = "Customer Id",
                                        ErrorMessage = "Value should be unique and not be same"
                                    }
                                }
                            });
                    }

                    if (request.ItemType
                        .ToLower()
                        .Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
                    {
                        failedItemTypeValidationBills = billPayments
                            ?.GroupBy(b => new { b.ItemCode, b.CustomerId })
                            .Where(g => g.Count() > 1)
                            .SelectMany(r => r);

                        foreach (var nonDistinct in failedItemTypeValidationBills)
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
                                        PropertyName = "Item Code and Customer Id",
                                        ErrorMessage = "Values should be unique."
                                    }
                                }
                            });
                    }

                    billPayments = billPayments
                        .Where(b => !failedItemTypeValidationBills.Any(n => n.RowNumber == b.RowNumber))
                        .Select(r => r);

                    uploadResult.ValidRows = billPayments.Select(r => new RowDetail
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
                throw new AppException(exception.Message, (int)HttpStatusCode.InternalServerError, uploadResult);
            }
        }

        public async Task<ValidateRowsResult<RowDetail>> ValidateContent(IEnumerable<Row> contentRows, ColumnContract[] columnContracts)
        {

            var validRows = new List<RowDetail>();
            ValidateRowModel validateRowModel;
            var failures = new List<Failure>();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(row, columnContracts);

                if (validateRowModel.IsValid)
                    validRows.Add(new RowDetail
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

            return new ValidateRowsResult<RowDetail> { Failures = failures, ValidRows = validRows };
        }

        private async Task<ValidateRowModel> ValidateRow(Row row, ColumnContract[] columnContracts)
        {
            var isValid = true;

            var validationErrors = GenericHelpers.ValidateRowCell(row, columnContracts, isValid);

            var failure = new Failure();

            var rowDetail = new RowDetail
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
                    new Failure
                    {
                        ColumnValidationErrors = validationErrors,
                        Row = rowDetail
                    };
            }

            return await Task.FromResult(new ValidateRowModel { IsValid = isValid, Failure = failure });
        }

        private string ConstructValidationError(Failure failure)
        {
            var result = new StringBuilder();
            for (int i = 0; i < failure.ColumnValidationErrors.Count(); i++)
            {
                result.Append($"{failure.ColumnValidationErrors[i].PropertyName}: {failure.ColumnValidationErrors[i].ErrorMessage}");

                if (failure.ColumnValidationErrors[i] != null)
                    result.Append(", ");
            }

            return result.ToString();
        }


    }

    public interface IFileContentValidator
    {
        Task<UploadResult> Validate(FileUploadRequest uploadRequest, IEnumerable<Row> rows);
    }
}
