using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Models;
using FilleUploadCore.Exceptions;
using FilleUploadCore.Helpers;
using FilleUploadCore.UploadManagers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public class GenericUploadService : IGenericUploadService
    {
        private readonly ILogger<GenericUploadService> _logger;
        private readonly IDbRepository _dbRepository;
        private readonly IHttpService _httpService;
        private readonly INasRepository _nasRepository;

        public GenericUploadService(ILogger<GenericUploadService> logger,
            IDbRepository dbRepository,
            IHttpService httpService,
            INasRepository nasRepository)
        {
            _logger = logger;
            _dbRepository = dbRepository;
            _httpService = httpService;
            _nasRepository = nasRepository;
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
                FileName = u.NameOfFile,
                ValidAmountSum = u.ValidAmountSum,
                ProductCode = u.ProductCode,
                ProductName = u.ProductName,
            });

            pagedData.TotalRowsCount = userFileSummaries.TotalRowsCount;

            return pagedData;
        }

        public async Task<PagedData<dynamic>> GetPaymentsStatus(string batchId, PaginationFilter pagination)
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

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.BillPaymentId)
                || fileSummary.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem))
                    paymentStatuses.Data = paymentStatus.RowStatusDto
                        .Select(s => new BillPaymentRowStatusUntyped
                        {
                            Amount = s.Amount,
                            CustomerId = s.CustomerId,
                            ItemCode = s.ItemCode,
                            ProductCode = s.ProductCode,
                            Error = s.Error,
                            Row = s.RowNum,
                            Status = s.RowStatus
                        });

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.Wht))
                    paymentStatuses.Data = paymentStatus.RowStatusDto
                        .Select(s => new FirsWhtRowStatusUntyped
                        {
                            BeneficiaryAddress = s.BeneficiaryAddress,
                            BeneficiaryName = s.BeneficiaryName,
                            BeneficiaryTin = s.BeneficiaryTin,
                            ContractAmount = s.ContractAmount,
                            ContractDate = s.ContractDate,
                            ContractDescription = s.ContractDescription,
                            ContractType = s.ContractType,
                            WhtRate = s.WhtRate,
                            WhtAmount = s.WhtAmount,
                            PeriodCovered = s.PeriodCovered,
                            InvoiceNumber = s.InvoiceNumber,
                            Error = s.Error,
                            Row = s.RowNum,
                            Status = s.RowStatus
                        });

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.Wvat))
                    paymentStatuses.Data = paymentStatus.RowStatusDto
                        .Select(s => new FirsWVatRowStatusUntyped
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

                if(fileSummary.ItemType.ToLower().Equals(GenericConstants.MultiTax))
                    paymentStatuses.Data = paymentStatus.RowStatusDto
                       .Select(s => new
                       {
                           s.BeneficiaryAddress,
                           s.BeneficiaryName,
                           s.BeneficiaryTin,
                           s.ContractAmount,
                           s.ContractDate,
                           s.ContractDescription,
                           s.ContractType,
                           s.WhtRate,
                           s.WhtAmount,
                           s.PeriodCovered,
                           s.InvoiceNumber,
                           s.Error,
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

            var confirmedBillPayments = await _dbRepository.GetConfirmedPayments(batchId);

            if (confirmedBillPayments.Count() <= 0 || !confirmedBillPayments.Any())
                throw new AppException($"Records awaiting payment initiation not found for batch Id: {batchId}", (int)HttpStatusCode.NotFound);

            var fileProperty = await _nasRepository.SaveFileToConfirmed(batchId, initiatePaymentOptions.ItemType, confirmedBillPayments);

            var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

            fileProperty.ContentType = fileSummary.ContentType;
            fileProperty.ItemType = fileSummary.ItemType;

            result = await _httpService.InitiatePayment(fileProperty, initiatePaymentOptions);

            await _dbRepository.UpdateBillPaymentInitiation(batchId);

            return result;
        }

        public async Task<string> GetFileTemplateContentAsync(string contentType, string itemType, MemoryStream outputStream)
        {
            string fileName;

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.Wht))
            {
                fileName = GenericConstants.FirsWhtCsvTemplate;
            }
            else if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.Wvat))
            {
                fileName = GenericConstants.FirsWvatCsvTemplate;
            }
            else if (contentType.ToLower().Equals(GenericConstants.BillPayment))
            {
                fileName = GenericConstants.BillPaymentCsvTemplate;
            }
            else
                throw new AppException("Template not found");

            await _nasRepository.GetTemplateFileContentAsync(fileName, outputStream);

            return fileName;
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
