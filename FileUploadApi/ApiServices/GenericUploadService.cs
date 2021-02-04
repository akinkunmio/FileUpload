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

        public async Task<PagedData<BatchFileSummaryDto>> GetUserFilesSummary(string userId, SummaryPaginationFilter paginationFilter)
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

        public async Task<PagedData<dynamic>> GetPaymentsStatus(string batchId, PaginationFilter pagination, string authToken)
        {
            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));
            ArgumentGuard.NotNullOrWhiteSpace(authToken, nameof(authToken));

            var paymentStatuses = new PagedData<dynamic>();

            try
            {
                var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

                if (GenericConstants.PendingValidation.Equals(fileSummary.TransactionStatus, StringComparison.InvariantCultureIgnoreCase)
                   && !GenericConstants.NoValidRecord.Equals(fileSummary.TransactionStatus, StringComparison.InvariantCultureIgnoreCase))
                {
                    await _httpService.ValidateRecords(new FileProperty
                    {
                        BatchId = batchId,
                        ContentType = fileSummary.ContentType,
                        ItemType = fileSummary.ItemType,
                        BusinessId = fileSummary.BusinessId,
                        BusinessTin = fileSummary.BusinessTin,
                        AdditionalData = fileSummary.AdditionalData,
                        Url = $"validate/{batchId}_validate.json"
                    }, authToken);

                    return paymentStatuses;
                }

                var paymentStatus = await _dbRepository.GetPaymentRowStatuses(batchId, pagination);
                var valid = paymentStatus.Where(x => x.RowStatus == "Valid").FirstOrDefault();
                
                paymentStatuses.TotalRowsCount = fileSummary.NumOfRecords;
                paymentStatuses.TotalAmountSum = fileSummary.ValidAmountSum;
                paymentStatuses.ConvenienceFee = fileSummary.ConvenienceFee;
                paymentStatuses.ProductCode = fileSummary.ProductCode;
                paymentStatuses.ProductName = fileSummary.ProductName;
                paymentStatuses.ItemType = fileSummary.ItemType;
                paymentStatuses.ContentType = fileSummary.ContentType;
                paymentStatuses.InvalidCount = fileSummary.NumOfRecords - fileSummary.NumOfValidRecords;
                paymentStatuses.ValidRowCount = fileSummary.NumOfValidRecords;
                paymentStatuses.FileName = fileSummary.NameOfFile;
                paymentStatuses.IsValidated = fileSummary.UploadSuccessful;

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.BillPaymentId)
                || fileSummary.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem))
                    paymentStatuses.Data = paymentStatus
                        .Select(s => new 
                        {
                            s.Amount,
                            s.CustomerId,
                            s.ItemCode,
                            s.ProductCode,
                            s.CustomerName,
                            s.Surcharge,
                            s.TransactionConvenienceFee,
                            s.BatchConvenienceFee,
                            ErrorDescription = s.Error,
                            Row = s.RowNum,
                            Status = s.RowStatus
                        });

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.Wht))
                    paymentStatuses.Data = paymentStatus
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
                            s.TransactionConvenienceFee,
                            s.BatchConvenienceFee,
                            ErrorDescription = s.Error,
                            Row = s.RowNum,
                            Status = s.RowStatus
                        });

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.Wvat))
                    paymentStatuses.Data = paymentStatus
                        .Select(s => new 
                        {
                            s.ContractorName,
                            s.ContractorAddress,
                            s.ContractDescription,
                            s.ContractorTin,
                            s.TransactionDate,
                            s.NatureOfTransaction,
                            s.InvoiceNumber,
                            s.TransactionCurrency,
                            s.CurrencyInvoicedValue,
                            s.TransactionInvoicedValue,
                            s.CurrencyExchangeRate,
                            s.TaxAccountNumber,
                            s.WVATRate,
                            s.WVATValue,
                            s.TransactionConvenienceFee,
                            s.BatchConvenienceFee,
                            ErrorDescription = s.Error,
                            Row = s.RowNum,
                            Status = s.RowStatus
                        });

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.MultiTax)
                    && fileSummary.ContentType.ToLower().Equals(GenericConstants.Firs))
                    paymentStatuses.Data = paymentStatus
                       .Select(s => new
                       {
                           Row = s.RowNum,
                           s.BeneficiaryTin,
                           s.BeneficiaryName,
                           s.BeneficiaryAddress,
                           s.ContractDate,
                           s.ContractDescription,
                           s.ContractAmount,
                           s.ContractType,
                           s.PeriodCovered,
                           s.InvoiceNumber,
                           s.WhtRate,
                           s.WhtAmount,
                           s.Amount,
                           s.Comment,
                           s.DocumentNumber,
                           s.PayerTin,
                           s.PayerName,
                           s.TransactionConvenienceFee,
                           s.BatchConvenienceFee,
                           s.TaxType,
                           ErrorDescription = s.Error,
                           Status = s.RowStatus
                       });
                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.SingleTax)
                    && fileSummary.ContentType.ToLower().Equals(GenericConstants.Firs))
                {
                    var taxTypeCode = paymentStatus.FirstOrDefault().TaxType.ToLower();
                    if (taxTypeCode.Equals(GenericConstants.Wht))
                        paymentStatuses.Data = paymentStatus
                        .Select(s => new
                        {
                            Row = s.RowNum,
                            s.BeneficiaryTin,
                            s.BeneficiaryName,
                            s.BeneficiaryAddress,
                            s.ContractDate,
                            s.ContractAmount,
                            s.ContractType,
                            s.PeriodCovered,
                            s.InvoiceNumber,
                            s.WhtRate,
                            s.WhtAmount,
                            s.TransactionConvenienceFee,
                            s.BatchConvenienceFee,
                            s.TaxType,
                            ErrorDescription = s.Error,
                            Status = s.RowStatus
                        });
                    else if (taxTypeCode.Equals(GenericConstants.Wvat))
                        paymentStatuses.Data = paymentStatus
                        .Select(s => new
                        {
                            Row = s.RowNum,
                            s.ContractorName,
                            s.ContractorAddress,
                            s.ContractorTin,
                            s.ContractDescription,
                            s.NatureOfTransaction,
                            s.TransactionDate,
                            s.TransactionCurrency,
                            s.CurrencyInvoicedValue,
                            s.CurrencyExchangeRate,
                            s.TransactionInvoicedValue,
                            s.WVATRate,
                            s.WVATValue,
                            s.TaxAccountNumber,
                            s.TransactionConvenienceFee,
                            s.BatchConvenienceFee,
                            s.TaxType,
                            ErrorDescription = s.Error,
                            Status = s.RowStatus
                        });
                    else
                        paymentStatuses.Data = paymentStatus
                        .Select(s => new
                        {
                            Row = s.RowNum,
                            s.Amount,
                            s.Comment,
                            s.DocumentNumber,
                            s.CustomerTin,
                            s.CustomerName,
                            s.TransactionConvenienceFee,
                            s.BatchConvenienceFee,
                            s.TaxType,
                            ErrorDescription = s.Error,
                            Status = s.RowStatus
                        });

                }

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.ManualCapture)
                    && fileSummary.ContentType.ToLower().Equals(GenericConstants.ManualCapture))
                    paymentStatuses.Data = paymentStatus
                       .Select(s => new
                       {
                           Row = s.RowNum,
                           s.ProductCode,
                           s.ItemCode,
                           s.CustomerId,
                           s.CustomerName,                          
                           s.Amount,
                           Desc = s.TaxType,
                           s.PhoneNumber,
                           s.Email,
                           s.Surcharge,
                           s.TransactionConvenienceFee,
                           s.BatchConvenienceFee,
                           Address = s.AddressInfo,
                           ErrorDescription = s.Error,
                           Status = s.RowStatus
                       });

                if (fileSummary.ItemType.ToLower().Equals(GenericConstants.Lasg)
                    && fileSummary.ContentType.ToLower().Equals(GenericConstants.Lasg))
                    paymentStatuses.Data = paymentStatus
                       .Select(s => new
                       {
                           Row = s.RowNum,
                           s.ProductCode,
                           s.ItemCode,
                           s.CustomerId,
                           s.PayerId,
                           s.RevenueCode,
                           s.AgencyCode,
                           s.CustomerName,
                           s.Surcharge,
                           s.TransactionConvenienceFee,
                           s.BatchConvenienceFee,
                           s.Amount,
                           s.StartPeriod,
                           s.EndPeriod,
                           Description = s.Narration,
                           ErrorDescription = s.Error,
                           Status = s.RowStatus
                       });
            }
            catch (AppException appEx)
            {
                throw appEx;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while getting bill payment statuses with error message {ex.Message} | {ex.StackTrace}");
                throw new AppException($"An error occured while fetching results for {batchId}!.");
            }

            return paymentStatuses;
        }

        public async Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions)
        {
            ConfirmedBillResponse result = default;

            var confirmedPayments = await _dbRepository.GetConfirmedPayments(batchId);

            if (confirmedPayments.Count() < 1 || !confirmedPayments.Any())
                throw new AppException($"No records found, to initiate payment on!.", (int)HttpStatusCode.BadRequest);

            var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

            var fileProperty = await _nasRepository.SaveFileToConfirmed(batchId, fileSummary.ContentType, fileSummary.ItemType, confirmedPayments, fileSummary.AdditionalData);

            fileProperty.ContentType = fileSummary.ContentType;
            fileProperty.ItemType = fileSummary.ItemType;

            initiatePaymentOptions.BusinessTin = fileSummary.BusinessTin ?? "";
            initiatePaymentOptions.TaxTypeName = fileSummary.AdditionalData ?? "";
            result = await _httpService.InitiatePayment(fileProperty, initiatePaymentOptions);

            await _dbRepository.UpdatePaymentInitiation(batchId);

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
            else if (contentType.ToLower().Equals(GenericConstants.BillPayment)
                && itemType.ToLower().Equals(GenericConstants.BillPayment))
            {
                fileName = GenericConstants.BillPaymentCsvTemplate;
            }
            else if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.MultiTax))
            {
                fileName = GenericConstants.FirsMultitaxPaymentCsvTemplate;
            }
            else if (contentType.ToLower().Equals(GenericConstants.ManualCapture)
               && itemType.ToLower().Equals(GenericConstants.ManualCapture))
            {
                fileName = GenericConstants.FctIrsCsvTemplate;
            }
            else if (contentType.ToLower().Equals(GenericConstants.Lasg)
               && itemType.ToLower().Equals(GenericConstants.Lasg))
            {
                fileName = GenericConstants.LasgCsvTemplate;
            }
            else if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.SingleTax))
            {
                fileName = GenericConstants.FirsSingletaxCsvTemplate;
            }
            else
                throw new AppException("Template not found!.");

            await _nasRepository.GetTemplateFileContentAsync(fileName, outputStream);

            return fileName;
        }

        public async Task<FileValidationResultModel> GetFileValidationResultAsync(string batchId, MemoryStream outputStream)
        {
            var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

            if (fileSummary == null)
                throw new AppException($"Batch Upload Summary for BatchId '{batchId}' not found!.", (int)HttpStatusCode.NotFound);

            if (string.IsNullOrWhiteSpace(fileSummary.NasUserValidationFile))
                throw new AppException($"Validation file not found for batch with Id '{batchId}'!.", (int)HttpStatusCode.NotFound);

            await _nasRepository.GetUserValidationResultAsync(fileSummary.NasUserValidationFile, outputStream);

            return new FileValidationResultModel
            {
                NasValidationFileName = fileSummary.NasUserValidationFile,
                RawFileName = fileSummary.NameOfFile
            };
        }

    }
}
