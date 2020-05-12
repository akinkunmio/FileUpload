using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.FileReaders;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using FileUploadApi.Models;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Repository;
using FileUploadApi.Controllers;
using System.Net;
using Microsoft.Extensions.Logging;

namespace FileUploadApi.ApiServices
{
    public class ApiUploadService : IApiUploadService
    {
        private readonly INasRepository _nasRepository;
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<ApiUploadService> _logger;

        public ApiUploadService( INasRepository nasRepository,
            IDbRepository dbRepository,
            ILogger<ApiUploadService> logger)
        {
            _nasRepository = nasRepository;
            _dbRepository = dbRepository;
            _logger = logger;
        }

        public async Task<BatchFileSummaryDto> GetFileSummary(string batchId)
        {
            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));

            BatchFileSummaryDto batchFileSummaryDto;

            batchFileSummaryDto = await _bulkBillPaymentService.GetBatchUploadSummary(batchId);
           
            return batchFileSummaryDto;
        }

        public async Task<PagedData<BatchFileSummaryDto>> GetUserFilesSummary(string userId, PaginationFilter paginationFilter)
        {
            ArgumentGuard.NotNullOrWhiteSpace(userId, nameof(userId));

            var userFileSummaries = new PagedData<BatchFileSummary>();
            var pagedData = new PagedData<BatchFileSummaryDto>();
            try
            {
                userFileSummaries = await _dbRepository.GetUploadSummariesByUserId(userId, paginationFilter);

                pagedData.Data = userFileSummaries.Data.Select(u => new BatchFileSummaryDto
                {
                    BatchId = u.BatchId,
                    ContentType = u.ContentType,
                    ItemType = u.ItemType,
                    NumOfAllRecords = u.NumOfRecords,
                    NumOfValidRecords = u.NumOfValidRecords,
                    Status = u.TransactionStatus,
                    UploadDate = u.UploadDate,
                    FileName = GenericHelpers.GetFileNameFromBatchId(u.BatchId)
                });

                pagedData.TotalRowsCount = userFileSummaries.TotalRowsCount;

            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while getting user upload summaries with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }

            return pagedData;
        }

        public async Task<PagedData<dynamic>> GetBillPaymentsStatus(string batchId, PaginationFilter pagination)
        {
            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));
            ArgumentGuard.NotNullOrWhiteSpace(pagination.ItemType, nameof(pagination.ItemType));

            var paymentStatuses = new PagedData<dynamic>();

            //IEnumerable<BillPayment> billPayments = new List<BillPayment>();
            //IEnumerable<BillPaymentRowStatus> billPaymentStatuses = default;
            int totalRowCount;
            double validAmountSum;

            try
            {
                var paymentStatus = await _dbRepository.GetPaymentRowStatuses(batchId, pagination);

                totalRowCount = paymentStatus.TotalRowsCount;

                validAmountSum = paymentStatus.ValidAmountSum;
                
                if(pagination.ItemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                || pagination.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
                paymentStatuses.Data = paymentStatus.RowStatusDto.Select(s => new BillPaymentRowStatusUntyped
                {
                    Amount = s.Amount,
                    CustomerId = s.CustomerId,
                    ItemCode = s.ItemCode,
                    ProductCode = s.ProductCode,
                    Error = s.Error,
                    Row = s.RowNumber,
                    Status = s.RowStatus
                });

                if (pagination.ItemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                    paymentStatuses.Data = paymentStatus.RowStatusDto.Select(s => new FirsWhtRowStatusUntyped
                    {
                        BeneficiaryAddress = s.BeneficiaryAddress,
                        BeneficiaryName = s.BeneficiaryName,
                        BeneficiaryTin = s.BeneficiaryTin,
                        ContractAmount = s.ContractAmount,
                        ContractDate = s.ContractDate,
                        ContractType = s.ContractType,
                        WhtRate = s.WhtRate,
                        WhtAmount = s.WhtAmount,
                        PeriodCovered = s.PeriodCovered,
                        InvoiceNumber = s.InvoiceNumber,
                        Error = s.Error,
                        Row = s.RowNumber,
                        Status = s.RowStatus
                    });

                if (pagination.ItemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                    paymentStatuses.Data = paymentStatus.RowStatusDto.Select(s => new FirsWVatRowStatusUntyped
                    {
                        ContractorName = s.ContractorName,
                        ContractorAddress = s.ContractorAddress,
                        ContractDescription = s.ContractDescription,
                        ContractorTin = s.ContractorTin,
                        TransactionDate = s.TransactionDate,
                        NatureOfTransaction = s.NatureOfTransaction,
                        InvoiceNumber = s.InvoiceNumber,
                        TransactionCurrency = s.TransactionCurrency,
                        CurrencyInvoicedValue = s.CurrencyInvoicedValue,
                        TransactionInvoicedValue = s.TransactionInvoicedValue,
                        CurrencyExchangeRate = s.CurrencyExchangeRate,
                        TaxAccountNumber = s.TaxAccountNumber,
                        WvatRate = s.WvatRate,
                        WvatValue = s.WvatValue,
                        Error = s.Error,
                        Row = s.RowNumber,
                        Status = s.RowStatus
                    });

                if (paymentStatuses.Data.Count() < 1)
                    throw new AppException($"Upload Batch Id '{batchId}' was not found", (int)HttpStatusCode.NotFound);
            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while getting bill payment statuses with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException($"An error occured while fetching results for {batchId}!.");
            }

            return paymentStatuses;
        }

        public async Task<UploadResult> UploadFileAsync(UploadOptions uploadOptions, Stream stream)
        {
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ContentType, nameof(uploadOptions.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ValidationType, nameof(uploadOptions.ValidationType));

            if (!uploadOptions.ValidationType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower())
                && !uploadOptions.ValidationType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                && !uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WVAT.ToLower())
                && !uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WHT.ToLower()))                
                throw new AppException("Invalid Validation Type specified");

            if (uploadOptions.UserId == null)
                throw new AppException("Id cannot be null");

            if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WHT.ToLower()) 
                || uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                uploadOptions.ContentType = GenericConstants.Firs;

            IEnumerable<Row> rows = new List<Row>();
            UploadResult uploadResult = new UploadResult();

            // uploadResult.BatchId = GenericHelpers.GenerateBatchId(uploadOptions.FileName, DateTime.Now, uploadOptions.ValidationType);
            var batchId = GenericHelpers.GenerateBatchId(uploadOptions.FileName, DateTime.Now, uploadOptions.ValidationType);

            switch (uploadOptions.FileExtension)
            {
                case "txt":
                    rows = _txtFileReader.Read(stream);
                    break;
                case "csv":
                    rows = _csvFileReader.Read(stream);
                    break;
                case "xlsx":
                    rows = _xlsxFileReader.Read(stream);
                    break;
                case "xls":
                    rows = _xlsFileReader.Read(stream);
                    break;
                default:
                    throw new AppException("File extension not supported!.");
            }

            switch (uploadOptions.ContentType.ToLower())
            {
                case "firs":
                    uploadResult = await _firsService.Upload(uploadOptions, rows, uploadResult);
                    break;
                case "autopay":
                    return await _autoPayService.Upload(uploadOptions, rows, uploadResult);
                case "sms":
                    return await _bulkSmsService.Upload(uploadOptions, rows, uploadResult);
                case "billpayment":
                    uploadResult = await _bulkBillPaymentService.Upload(uploadOptions, rows, uploadResult);
                    break;
                default:
                    throw new AppException("Content type not supported!.");
            }

            return uploadResult;
        }

        public async Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions)
        {
            try
            {
                ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));
                ArgumentGuard.NotNull(initiatePaymentOptions.BusinessId, nameof(initiatePaymentOptions.BusinessId));
                ArgumentGuard.NotNull(initiatePaymentOptions.ApprovalConfigId, nameof(initiatePaymentOptions.ApprovalConfigId));
                ArgumentGuard.NotNull(initiatePaymentOptions.UserId, nameof(initiatePaymentOptions.UserId));
                ArgumentGuard.NotNullOrWhiteSpace(initiatePaymentOptions.UserName, nameof(initiatePaymentOptions.UserName));

                return await _bulkBillPaymentService.PaymentInitiationConfirmed(batchId, initiatePaymentOptions);
            }
            catch(AppException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetFileTemplateContentAsync(string extension, MemoryStream outputStream)
        {
            string templateFileName;

            try
            {
                switch (extension.ToLower())
                {
                    case "txt":
                        templateFileName = GenericConstants.BillPaymentTxtTemplate;
                        break;
                    case "csv":
                        templateFileName = GenericConstants.BillPaymentCsvTemplate;
                        break;
                    case "xlsx":
                        templateFileName = GenericConstants.BillPaymentXlsxTemplate;
                        break;
                    case "xls":
                        templateFileName = GenericConstants.BillPaymentXlsTemplate;
                        break;
                    default:
                        throw new AppException("File extension not supported!.");
                }
                return await _nasRepository.GetTemplateFileContentAsync(templateFileName, outputStream);
            }
            catch(AppException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetFileValidationResultAsync(string batchId, MemoryStream outputStream)
        {
            var batchFileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

            if (batchFileSummary == null)
                throw new AppException($"Batch Upload Summary for BatchId: {batchId} not found", (int)HttpStatusCode.NotFound);

            if(string.IsNullOrWhiteSpace(batchFileSummary.NasUserValidationFile))
                throw new AppException($"Validation file not found for batch with Id : {batchId}", (int)HttpStatusCode.NotFound);

            await _nasRepository.GetUserValidationResultAsync(batchFileSummary.NasUserValidationFile, outputStream);

            return batchFileSummary.NasUserValidationFile;
        }
    }

}
