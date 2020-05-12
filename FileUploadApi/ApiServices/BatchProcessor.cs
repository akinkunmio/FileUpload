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
            
            var uploadResult = new UploadResult
            { 
                ProductCode = request.ProductCode, 
                ProductName = request.ProductName, 
                FileName = request.FileName 
            };

            uploadResult.BatchId = GenericHelpers.GenerateBatchId(request.FileName, DateTime.Now);

            using (var contentStream = request.FileRef.OpenReadStream())
            {
                IEnumerable<Row> rows = GetRows(request.FileExtension, contentStream);

                await ValidateFileContentAsync(request, rows, uploadResult);

                await _batchRepository.Save(uploadResult, request);

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
                RecordsCount = batchFileSummary.NumOfRecords,
                ValidRecordsCount = batchFileSummary.NumOfValidRecords,
                Status = batchFileSummary.TransactionStatus,
                UploadDate = batchFileSummary.UploadDate
            };

            return batchFileSummaryDto;
        }

        public async Task<PagedData<BatchFileSummaryDto>> GetUserFilesSummary(string userId, PaginationFilter paginationFilter)
        {
            var userFileSummaries = new PagedData<BatchFileSummary>();
            var pagedData = new PagedData<BatchFileSummaryDto>();

            userFileSummaries = await _dbRepository.GetUploadSummariesByUserId(userId, paginationFilter);

            pagedData.Data = userFileSummaries.Data.Select(u => new BatchFileSummaryDto
            {
                BatchId = u.BatchId,
                ContentType = u.ContentType,
                ItemType = u.ItemType,
                RecordsCount = u.NumOfRecords,
                ValidRecordsCount = u.NumOfValidRecords,
                InvalidRecordsCount = u.NumOfRecords - u.NumOfValidRecords,
                Status = u.TransactionStatus,
                UploadDate = u.UploadDate,
                FileName = u.NameOfFile ?? GenericHelpers.GetFileNameFromBatchId(u.BatchId),
                ValidAmountSum = u.ValidAmountSum,
                ProductCode = u.ProductCode,
                ProductName = u.ProductName,
            });

            pagedData.TotalRowsCount = userFileSummaries.TotalRowsCount;

            return pagedData;
        }

        public async Task<PagedData<dynamic>> GetBillPaymentsStatus(string batchId, PaginationFilter pagination)
        {
            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));
            
            var paymentStatuses = new PagedData<dynamic>();

            try
            {
                var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

                var paymentStatus = await _dbRepository.GetPaymentRowStatuses(batchId, pagination);

                paymentStatuses.TotalRowsCount = fileSummary.NumOfRecords;
                paymentStatuses.TotalAmountSum = fileSummary.ValidAmountSum;
                paymentStatuses.ProductCode = fileSummary.ProductCode;
                paymentStatuses.ProductName = fileSummary.ProductName;
                paymentStatuses.ItemType = fileSummary.ItemType;
                paymentStatuses.ContentType = fileSummary.ContentType;
                paymentStatuses.InvalidCount = fileSummary.NumOfRecords - fileSummary.NumOfValidRecords;
                paymentStatuses.ValidRowCount = fileSummary.NumOfValidRecords;
                paymentStatuses.FileName = fileSummary.NameOfFile;

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                || fileSummary.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
                    paymentStatuses.Data = paymentStatus.RowStatusDto.Select(s => new BillPaymentRowStatusUntyped
                    {
                        Amount = s.Amount,
                        CustomerId = s.CustomerId,
                        ItemCode = s.ItemCode,
                        ProductCode = s.ProductCode,
                        Error = s.Error,
                        Row = s.RowNum,
                        Status = s.RowStatus
                    });

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.WHT))
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
                        Row = s.RowNum,
                        Status = s.RowStatus
                    });

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.WVAT))
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
                        Row = s.RowNum,
                        Status = s.RowStatus
                    });

                if (paymentStatuses.Data.Count() < 1)
                    throw new AppException($"No result found for Batch Id '{batchId}'", (int)HttpStatusCode.NotFound);
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

            var confirmedBillPayments = await _dbRepository.GetConfirmedBillPayments(batchId);

            if (confirmedBillPayments.Count() <= 0 || !confirmedBillPayments.Any())
                throw new AppException($"Records awaiting payment initiation not found for batch Id: {batchId}", (int)HttpStatusCode.NotFound);

            var fileProperty = await _nasRepository.SaveFileToConfirmed(batchId, initiatePaymentOptions.ItemType, confirmedBillPayments);

            var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

            fileProperty.ContentType = fileSummary.ContentType;
            fileProperty.ItemType = fileSummary.ItemType;

            result = await _httpService.ConfirmedBillRecords(fileProperty, initiatePaymentOptions);

            await _dbRepository.UpdateBillPaymentInitiation(batchId);

            return result;
        }

        public async Task<FileTemplateModel> GetFileTemplateContentAsync(string contentType, string itemType, MemoryStream outputStream)
        {
            var result = new FileTemplateModel();

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.WHT))
                result.FileName = GenericConstants.FirsWhtCsvTemplate;

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.WHT))
                result.FileName = GenericConstants.FirsWvatCsvTemplate;

            if (contentType.ToLower().Equals(GenericConstants.BillPayment))
                result.FileName = GenericConstants.BillPaymentCsvTemplate;

            result.FilePath = await _nasRepository.GetTemplateFileContentAsync(result.FileName, outputStream);

            return result;
        }

        public async Task<FileValidationResultModel> GetFileValidationResultAsync(string batchId, MemoryStream outputStream)
        {
            var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

            if (fileSummary == null)
                throw new AppException($"Batch Upload Summary for BatchId: {batchId} not found", (int)HttpStatusCode.NotFound);

            if (string.IsNullOrWhiteSpace(fileSummary.NasUserValidationFile))
                throw new AppException($"Validation file not found for batch with Id : {batchId}", (int)HttpStatusCode.NotFound);

            await _nasRepository.GetUserValidationResultAsync(fileSummary.NasUserValidationFile, outputStream);

            return new FileValidationResultModel 
            { 
                NasValidationFileName = fileSummary.NasUserValidationFile, 
                RawFileName = fileSummary.NameOfFile 
            };
        }

    }
}
