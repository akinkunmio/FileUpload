using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.FileReaders;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
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
using Microsoft.AspNetCore.Http;
using FileUploadAndValidation.FileServices;
using Microsoft.Extensions.Logging;

namespace FileUploadApi.ApiServices
{
    public class BatchProcessor : IBatchProcessor
    {
        private readonly INasRepository _nasRepository;
        private readonly IHttpService _httpService;
        private readonly IBatchRepository _batchRepository;
        private readonly IEnumerable<IFileContentValidator> _fileContentValidators;
        private readonly IEnumerable<IFileReader> _fileReaders;
        private readonly ILogger<BatchProcessor> _logger;
        private readonly IDbRepository _dbRepository;



        public BatchProcessor(IBatchRepository batchRepository,
            IEnumerable<IFileContentValidator> fileContentValidators,
            IEnumerable<IFileReader> fileReaders,
            ILogger<BatchProcessor> logger,
            IDbRepository dbRepository,
            IHttpService httpService,
            INasRepository nasRepository)
        {
            _batchRepository = batchRepository;
            _fileContentValidators = fileContentValidators;
            _fileReaders = fileReaders;
            _logger = logger;
            _dbRepository = dbRepository;
            _httpService = httpService;
            _nasRepository = nasRepository;
        }

        public async Task<UploadResult> UploadFileAsync(FileUploadRequest request)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(request.ItemType, nameof(request.ItemType));

            if (!request.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower())
                && !request.ItemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                && !request.ItemType.ToLower().Equals(GenericConstants.WVAT.ToLower())
                && !request.ItemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                throw new AppException("Invalid Content Type specified");

            if (request.ContentType.ToLower().Equals(GenericConstants.Firs.ToLower()))
                ArgumentGuard.NotNullOrWhiteSpace(request.BusinessTin, nameof(request.BusinessTin));
            var uploadResult = new UploadResult();

            var batchId = GenericHelpers.GenerateBatchId(request.FileName, DateTime.Now);

            using (var contentStream = request.FileRef.OpenReadStream())
            {
                IEnumerable<Row> rows = GetRows(request.FileExtension, contentStream);

                uploadResult = await ValidateFileContentAsync(request, rows, uploadResult);

                await _batchRepository.Save(batchId, request, uploadResult.ValidRows, uploadResult.Failures);

                return uploadResult;
            }
        }

        private IEnumerable<Row> GetRows(string fileExtension, Stream contentStream)
        {
            switch (fileExtension)
            {
                case "txt":
                    return _fileReaders.ToArray()[0].Read(contentStream);
                case "csv":
                    return _fileReaders.ToArray()[1].Read(contentStream);
                case "xlsx":
                    return _fileReaders.ToArray()[2].Read(contentStream);
                case "xls":
                    return _fileReaders.ToArray()[3].Read(contentStream);
                default:
                    throw new AppException("File extension not supported!.");
            }
        }

        private async Task<UploadResult> ValidateFileContentAsync(FileUploadRequest request, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            switch (request.ContentType.ToLower())
            {
                case "billpayment":
                    return await _fileContentValidators.ToArray()[0].Validate(request, rows, uploadResult);
                case "firs":
                    return await _fileContentValidators.ToArray()[1].Validate(request, rows, uploadResult);
                default:
                    throw new AppException("Content type not supported!.");
            }
        }

        public async Task<BatchFileSummaryDto> GetFileSummary(string batchId)
        {
            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));

            BatchFileSummaryDto batchFileSummaryDto;

            var batchFileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

            if (batchFileSummary == null)
                throw new AppException($"Upload with Batch Id: '{batchId}' not found!.", (int)HttpStatusCode.NotFound);

            batchFileSummaryDto = new BatchFileSummaryDto
            {
                BatchId = batchFileSummary.BatchId,
                ContentType = batchFileSummary.ContentType,
                ItemType = batchFileSummary.ItemType,
                NumOfAllRecords = batchFileSummary.NumOfRecords,
                NumOfValidRecords = batchFileSummary.NumOfValidRecords,
                Status = batchFileSummary.TransactionStatus,
                UploadDate = batchFileSummary.UploadDate
            };

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
                    FileName = u.FileName ?? GenericHelpers.GetFileNameFromBatchId(u.BatchId)
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
            ArgumentGuard.NotNullOrWhiteSpace(pagination.ContentType, nameof(pagination.ContentType));

            var paymentStatuses = new PagedData<dynamic>();

            //IEnumerable<BillPayment> billPayments = new List<BillPayment>();
            //IEnumerable<BillPaymentRowStatus> billPaymentStatuses = default;
            //int totalRowCount;
            //double validAmountSum;
            try
            {
                var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

                var paymentStatus = await _dbRepository.GetPaymentRowStatuses(batchId, pagination);

                paymentStatuses.TotalRowsCount = paymentStatus.TotalRowsCount;
                paymentStatuses.TotalAmountSum = paymentStatus.ValidAmountSum;
                paymentStatuses.ProductCode = fileSummary.ProductCode ?? "";
                paymentStatuses.ProductName = fileSummary.ProductName ??  "";
                paymentStatuses.FileName = fileSummary.FileName ?? GenericHelpers.GetFileNameFromBatchId(batchId);

                if (pagination.ItemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
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

        public async Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions)
        {
            ConfirmedBillResponse result;
            try
            {
                var confirmedBillPayments = await _dbRepository.GetConfirmedBillPayments(batchId);

                if (confirmedBillPayments.Count() < 0 || !confirmedBillPayments.Any())
                    throw new AppException($"Records awaiting payment initiation not found for batch Id: {batchId}", (int)HttpStatusCode.NotFound);

                var fileProperty = await _nasRepository.SaveFileToConfirmed(batchId, initiatePaymentOptions.ItemType, confirmedBillPayments);

                var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

                fileProperty.ContentType = fileSummary.ContentType;
                fileProperty.ItemType = fileSummary.ItemType;

                result = await _httpService.ConfirmedBillRecords(fileProperty, initiatePaymentOptions);

                await _dbRepository.UpdateBillPaymentInitiation(batchId);
            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while while initiating payment with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException("An error occured while initiating payment");
            }

            return result;
        }

        public async Task<string> GetFileTemplateContentAsync(string itemType, MemoryStream outputStream)
        {
            string templateFileName;

            try
            {
                switch (itemType.ToLower())
                {
                    case "billpayment":
                        templateFileName = GenericConstants.BillPaymentTxtTemplate;
                        break;
                    case "wht":
                        templateFileName = GenericConstants.BillPaymentCsvTemplate;
                        break;
                    case "wvat":
                        templateFileName = GenericConstants.BillPaymentXlsxTemplate;
                        break;
                    default:
                        throw new AppException("File type not supported!.");
                }
                return await _nasRepository.GetTemplateFileContentAsync(templateFileName, outputStream);
            }
            catch (AppException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetFileValidationResultAsync(string batchId, MemoryStream outputStream)
        {
            var batchFileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

            if (batchFileSummary == null)
                throw new AppException($"Batch Upload Summary for BatchId: {batchId} not found", (int)HttpStatusCode.NotFound);

            if (string.IsNullOrWhiteSpace(batchFileSummary.NasUserValidationFile))
                throw new AppException($"Validation file not found for batch with Id : {batchId}", (int)HttpStatusCode.NotFound);

            await _nasRepository.GetUserValidationResultAsync(batchFileSummary.NasUserValidationFile, outputStream);

            return batchFileSummary.NasUserValidationFile;
        }

    }
}
