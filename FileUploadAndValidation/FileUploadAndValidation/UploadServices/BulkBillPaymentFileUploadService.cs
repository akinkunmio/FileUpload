using FileUploadAndValidation;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadAndValidation.Utils;
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

        public async Task<UploadResult> ValidateContent(IEnumerable<Row> contentRows, UploadResult uploadResult)
        {
            Console.WriteLine("Validating rows...");

            foreach (var row in contentRows)
            {
                var isValidRow = await ValidateRow(row, uploadResult);
                if (isValidRow)
                    uploadResult.ValidRows.Add(row.Index);
            }

            return uploadResult;
        }

        private async Task<bool> ValidateRow(Row row, UploadResult uploadResult)
        {
            var validationErrors = new List<ValidationError>();

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
                uploadResult.Failures.Add(
                    new Failure
                    {
                        ColumnValidationErrors = validationErrors,
                        RowNumber = row.Index
                    });
            }

            return isValid;
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

                //if (uploadOptions.ItemType.ToUpper().Equals(GenericConstants.BillPaymentIdPlusItem))
                await ValidateContent(contentRows, uploadResult);

                var batchId = GenerateBatchId();

                var dateTimeNow = DateTime.Now.ToString();
                if (uploadResult.ValidRows.Count() > 0 || uploadResult.ValidRows.Any())
                {
                    billPayments = rows
                        .Where(row => uploadResult.ValidRows.Contains(row.Index))
                        .Select(r => new BillPayment
                        {
                            ProductCode = r.Columns[0].Value.ToString(),
                            ItemCode = r.Columns[1].Value.ToString(),
                            CustomerId = r.Columns[2].Value.ToString(),
                            Amount = Convert.ToInt32(r.Columns[3].Value),
                            BatchId = batchId,
                            CreatedDate = dateTimeNow
                        });
                }
                await _dbRepository.InsertPaymentUpload(
                    new BatchFileSummary
                    {
                        BatchId = batchId,
                        NumOfAllRecords = uploadResult.RowsCount,
                        NumOfValidRecords = uploadResult.ValidRows.Count(),
                        Status = GenericConstants.PendingValidation,
                        UploadDate = dateTimeNow,
                        CustomerFileName = uploadOptions.FileName
                    }, billPayments.ToList());


                var fileProperties = await _nasRepository.SaveAsJsonFile(batchId, billPayments);

                //  var validateResponse = await _billPaymentService
                //    .ValidateBillRecords(fileProperties.FileName, fileProperties.FileLocation, uploadOptions.AuthToken);

                uploadResult.ScheduleId = batchId;
                return uploadResult;
            }
            catch (Exception exception)
            {
                uploadResult.ErrorMessage = exception.Message;
                return uploadResult;
            }
        }

        private string GenerateBatchId()
        {
            return Guid.NewGuid().ToString() + "|" + DateTime.Now.ToString("yyyyMMddHHMMss");
        }
    }
}