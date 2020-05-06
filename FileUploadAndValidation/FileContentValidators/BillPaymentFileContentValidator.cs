﻿using FileUploadAndValidation.Helpers;
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
    public class BillPaymentFileContentValidator : IFileContentValidator
    {
        private readonly ILogger<BillPaymentFileContentValidator> _logger;
        public BillPaymentFileContentValidator(ILogger<BillPaymentFileContentValidator> logger)
        {
            _logger = logger;
        }

        public async Task<UploadResult> Validate(FileUploadRequest request, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(request.ItemType, nameof(request.ItemType));
            ArgumentGuard.NotNullOrEmpty(rows, nameof(rows));

            var headerRow = new Row();
            IEnumerable<RowDetail> billPayments = new List<RowDetail>();
            IEnumerable<RowDetail> failedItemTypeValidationBills = new List<RowDetail>();

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
                    var productCodeList = uploadResult.ValidRows.Select(s => s.ProductCode).ToArray();

                    string firstItem = productCodeList[0];

                    if (!string.Equals(firstItem, request.ProductCode, StringComparison.InvariantCultureIgnoreCase))
                        throw new AppException($"Expected file Product Code to be {request.ProductCode}, but found {firstItem}!.");

                    bool allEqual = productCodeList.Skip(1)
                      .All(s => string.Equals(firstItem, s, StringComparison.InvariantCultureIgnoreCase));

                    if (!allEqual)
                        throw new AppException("Product Code should have same value for all records");

                    if (request.ItemType
                        .ToLower()
                        .Equals(GenericConstants.BillPaymentId.ToLower()))
                    {
                        failedItemTypeValidationBills = uploadResult.ValidRows
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
                        failedItemTypeValidationBills = uploadResult.ValidRows
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

                    uploadResult.ValidRows = uploadResult.ValidRows
                        .Where(b => !failedItemTypeValidationBills.Any(n => n.RowNumber == b.RowNumber))
                        .Select(r => r).ToList();

                }

                uploadResult.Failures = uploadResult.Failures.Select(f => new Failure 
                { 
                    Row = new RowDetail
                    {
                        Amount = f.Row.Amount,
                        CreatedDate = dateTimeNow.ToString(),
                        CustomerId = f.Row.CustomerId,
                        ItemCode = f.Row.ItemCode,
                        ProductCode = f.Row.ProductCode,
                        RowNumber = f.Row.RowNumber,
                        Error = GenericHelpers.ConstructValidationError(f)
                    }
                }).ToList();

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

        public async Task<ValidateRowsResult> ValidateContent(IEnumerable<Row> contentRows, ColumnContract[] columnContracts)
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

            return new ValidateRowsResult { Failures = failures, ValidRows = validRows };
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

       
    }

}
