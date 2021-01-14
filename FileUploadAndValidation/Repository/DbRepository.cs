using FileUploadAndValidation.Models;
using FileUploadApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using FileUploadAndValidation.Utils;
using System.Data.SqlClient;
using static Dapper.DefaultTypeMap;
using System.Data;
using FilleUploadCore.Exceptions;
using System.Net;
using FileUploadAndValidation.Helpers;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace FileUploadAndValidation.Repository
{
    public class DbRepository : IDbRepository
    {
        private readonly IAppConfig _appConfig;
        private readonly ILogger<DbRepository> _logger;

        public DbRepository(IAppConfig appConfig, ILogger<DbRepository> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
        }
        
        public async Task<string> InsertAllUploadRecords(UploadSummaryDto fileDetail, IList<RowDetail> payments, IList<Failure> invalidPayments)
        {
            var allpayments = payments.Concat(invalidPayments.Select(a => a.Row));

            try
            {
                using (var connection = new SqlConnection(_appConfig.UploadServiceConnectionString))
                {
                    connection.Open();

                    using (var sqlTransaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var transactionSummaryId = await connection.ExecuteScalarAsync(sql: "sp_insert_payment_transaction_summary",
                               param: new
                               {
                                   batch_id = fileDetail.BatchId,
                                   status = fileDetail.Status,
                                   item_type = fileDetail.ItemType,
                                   num_of_records = fileDetail.NumOfAllRecords,
                                   upload_date = fileDetail.UploadDate,
                                   content_type = fileDetail.ContentType,
                                   userid = fileDetail.UserId,
                                   businessid = fileDetail.BusinessId,
                                   product_code = fileDetail.ProductCode,
                                   product_name = fileDetail.ProductName,
                                   file_name = fileDetail.CustomerFileName
                               },
                               transaction: sqlTransaction,
                               commandType: System.Data.CommandType.StoredProcedure);

                            if (fileDetail.ContentType.ToLower().Equals(GenericConstants.BillPayment) 
                                && (fileDetail.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem) 
                                || fileDetail.ItemType.ToLower().Equals(GenericConstants.BillPaymentId)))
                            {
                                

                                foreach (var payment in allpayments)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_invalid_bill_payments",
                                        param: new
                                        {
                                            product_code = payment.ProductCode,
                                            item_code = payment.ItemCode,
                                            customer_id = payment.CustomerId,
                                            amount = payment.Amount,
                                            row_num = payment.RowNum,
                                            created_date = payment.CreatedDate,
                                            transactions_summary_Id = transactionSummaryId,
                                            row_status = string.IsNullOrWhiteSpace(payment.ErrorDescription) ? "" : "Invalid",
                                            initial_validation_status = string.IsNullOrWhiteSpace(payment.ErrorDescription) ? "Valid" : "Invalid",
                                            error = payment.ErrorDescription ?? ""
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }
                            }

                            if (fileDetail.ContentType.ToLower().Equals(GenericConstants.Firs)
                                && fileDetail.ItemType.ToLower().Equals(GenericConstants.Wht))
                            {
                                foreach (var valid in payments)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_valid_firs_wht",
                                        param: new
                                        {
                                            beneficiary_address = valid.BeneficiaryAddress,
                                            beneficiary_name = valid.BeneficiaryName,
                                            beneficiary_tin = valid.BeneficiaryTin,
                                            contract_amount = valid.ContractAmount,
                                            contract_description = valid.ContractDescription,
                                            contract_date = valid.ContractDate,
                                            contract_type = valid.ContractType,
                                            wht_rate = valid.WhtRate,
                                            wht_amount = valid.WhtAmount,
                                            period_covered = valid.PeriodCovered,
                                            invoice_number = valid.InvoiceNumber,
                                            created_date = valid.CreatedDate,
                                            row_num = valid.RowNum,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Valid"
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }

                                foreach (var invalid in invalidPayments)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_invalid_firs_wht",
                                        param: new
                                        {
                                            beneficiary_address = invalid.Row.BeneficiaryAddress,
                                            beneficiary_name = invalid.Row.BeneficiaryName,
                                            beneficiary_tin = invalid.Row.BeneficiaryTin,
                                            contract_amount = invalid.Row.ContractAmount,
                                            contract_description = invalid.Row.ContractDescription,
                                            contract_date = invalid.Row.ContractDate,
                                            contract_type = invalid.Row.ContractType,
                                            wht_rate = invalid.Row.WhtRate,
                                            wht_amount = invalid.Row.WhtAmount,
                                            period_covered = invalid.Row.PeriodCovered,
                                            invoice_number = invalid.Row.InvoiceNumber,
                                            row_status = "Invalid",
                                            created_date = invalid.Row.CreatedDate,
                                            row_num = invalid.Row.RowNum,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Invalid",
                                            error = invalid.Row.ErrorDescription
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }
                            }

                            if (fileDetail.ContentType.ToLower().Equals(GenericConstants.Firs)
                                && fileDetail.ItemType.ToLower().Equals(GenericConstants.Wvat))
                            {
                                foreach (var validWht in payments)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_valid_firs_wvat",
                                        param: new
                                        {
                                            contractor_name = validWht.ContractorName,
                                            contractor_address = validWht.ContractorAddress,
                                            contract_description = validWht.ContractDescription,
                                            contractor_tin = validWht.ContractorTin,
                                            transaction_date = validWht.TransactionDate,
                                            nature_of_transaction = validWht.NatureOfTransaction,
                                            invoice_number = validWht.InvoiceNumber,
                                            transaction_currency = validWht.TransactionCurrency,
                                            currency_invoiced_value = validWht.CurrencyInvoicedValue,
                                            transaction_invoiced_value = validWht.TransactionInvoicedValue,
                                            currency_exchange_rate = validWht.CurrencyExchangeRate,
                                            tax_account_number = validWht.TaxAccountNumber,
                                            wvat_rate = validWht.WvatRate,
                                            wvat_value = validWht.WvatValue,
                                            created_date = validWht.CreatedDate,
                                            row_num = validWht.RowNum,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Valid"
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }

                                foreach (var invalidWvat in invalidPayments)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_invalid_firs_wvat",
                                        param: new
                                        {
                                            contractor_name = invalidWvat.Row.ContractorName,
                                            contractor_address = invalidWvat.Row.ContractorAddress,
                                            contract_description = invalidWvat.Row.ContractDescription,
                                            contractor_tin = invalidWvat.Row.ContractorTin,
                                            transaction_date = invalidWvat.Row.TransactionDate,
                                            nature_of_transaction = invalidWvat.Row.NatureOfTransaction,
                                            invoice_number = invalidWvat.Row.InvoiceNumber,
                                            transaction_currency = invalidWvat.Row.TransactionCurrency,
                                            currency_invoiced_value = invalidWvat.Row.CurrencyInvoicedValue,
                                            transaction_invoiced_value = invalidWvat.Row.TransactionInvoicedValue,
                                            currency_exchange_rate = invalidWvat.Row.CurrencyExchangeRate,
                                            tax_account_number = invalidWvat.Row.TaxAccountNumber,
                                            wvat_rate = invalidWvat.Row.WvatRate,
                                            wvat_value = invalidWvat.Row.WvatValue,
                                            created_date = invalidWvat.Row.CreatedDate,
                                            row_num = invalidWvat.Row.RowNum,
                                            transactions_summary_Id = transactionSummaryId,
                                            row_status = "Invalid",
                                            initial_validation_status = "Invalid",
                                            error = invalidWvat.Row.ErrorDescription
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }
                            }

                            if(fileDetail.ContentType.ToLower().Equals(GenericConstants.Firs)
                                && fileDetail.ItemType.ToLower().Equals(GenericConstants.MultiTax))
                            {
                               

                                foreach (var payment in allpayments)
                                {
                                    // create sp_insert_invalid_firs_multitax
                                    await connection.ExecuteAsync(sql: "sp_insert_invalid_firs_multitax",
                                        param: new
                                        {
                                            beneficiary_address = payment.BeneficiaryAddress,
                                            beneficiary_name = payment.BeneficiaryName,
                                            beneficiary_tin = payment.BeneficiaryTin,
                                            contract_amount = payment.ContractAmount,
                                            contract_date = payment.ContractDate,
                                            contract_type = payment.ContractType,
                                            contract_description = payment.ContractDescription,
                                            invoice_number = payment.InvoiceNumber,
                                            wht_rate = payment.WhtRate,
                                            wht_amount = payment.WhtAmount,
                                            period_covered = payment.PeriodCovered,
                                            amount = payment.Amount,
                                            comment = payment.Comment,
                                            tax_type = payment.TaxType,
                                            document_number = payment.DocumentNumber,
                                            payer_tin = payment.PayerTin,
                                            created_date = payment.CreatedDate,
                                            row_num = payment.RowNum,
                                            transactions_summary_Id = transactionSummaryId,
                                            row_status = string.IsNullOrWhiteSpace(payment.ErrorDescription) ? "" : "Invalid",
                                            initial_validation_status = string.IsNullOrWhiteSpace(payment.ErrorDescription) ? "Valid" : "Invalid",
                                            error = payment.ErrorDescription ?? ""
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }
                            }

                            if (fileDetail.ContentType.ToLower().Equals(GenericConstants.FctIrs)
                              && fileDetail.ItemType.ToLower().Equals(GenericConstants.MultiTax))
                            {
                                foreach (var valid in payments)
                                {
                                    //create sp sp_insert_valid_fctirs_multitax
                                    await connection.ExecuteAsync(sql: "sp_insert_valid_fctirs_multitax",
                                        param: new
                                        {
                                            product_code = valid.ProductCode,
                                            item_code = valid.ItemCode,
                                            customer_id = valid.CustomerId,
                                            amount = valid.Amount,
                                            desc = valid.Desc,
                                            customer_name = valid.CustomerName,
                                            phone_number = valid.PhoneNumber,
                                            email = valid.Email,
                                            address = valid.Address,
                                            created_date = valid.CreatedDate,
                                            row_num = valid.RowNum,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Valid"
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }

                                foreach (var inValid in invalidPayments)
                                {
                                    // create sp_insert_invalid_fctirs_multitax
                                    await connection.ExecuteAsync(sql: "sp_insert_invalid_fctirs_multitax",
                                        param: new
                                        {
                                            product_code = inValid.Row.ProductCode,
                                            item_code = inValid.Row.ItemCode,
                                            customer_id = inValid.Row.CustomerId,
                                            amount = inValid.Row.Amount,
                                            desc = inValid.Row.Desc,
                                            customer_name = inValid.Row.CustomerName,
                                            phone_number = inValid.Row.PhoneNumber,
                                            email = inValid.Row.Email,
                                            address = inValid.Row.Address,
                                            created_date = inValid.Row.CreatedDate,
                                            row_num = inValid.Row.RowNum,
                                            transactions_summary_Id = transactionSummaryId,
                                            row_status = "Invalid",
                                            initial_validation_status = "Invalid",
                                            error = inValid.Row.ErrorDescription
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }
                            }

                            sqlTransaction.Commit();
                            return fileDetail.BatchId;
                        }
                        catch (Exception exception)
                        {
                            sqlTransaction.Rollback();
                            throw exception;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while inserting payment items in database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException("An error occured. Please, retry!.", 400);
            }
        }

        public async Task UpdateUploadSuccess(string batchId, string userValidationFileName)
        {
             using (var connection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                connection.Open();

                try
                {
                    BatchFileSummary fileSummary = await GetBatchUploadSummary(batchId);

                    await connection.ExecuteAsync(
                        sql: "sp_update_successful_upload",
                        param: new
                        {
                            batch_id = batchId,
                            nas_uservalidationfile = userValidationFileName
                        },
                        commandType: CommandType.StoredProcedure);
                }
                catch (AppException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured while performing Update Successful Upload operation from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw new AppException("An error occured. Please, retry!.", 400);
                }
            }
        }

        public async Task<BatchFileSummary> GetBatchUploadSummary(string batchId)
        {

            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;
                try
                {
                    var batchFileSummary = await sqlConnection.QueryFirstOrDefaultAsync<BatchFileSummary>(
                        sql: @"sp_get_batch_upload_summary_by_batch_id",
                        param: new
                        {
                            batch_id = batchId
                        },
                        commandType: CommandType.StoredProcedure);

                    return batchFileSummary ?? throw new AppException($"Upload file with Batch Id: '{batchId}' not found!.", (int)HttpStatusCode.NotFound);
                }
                catch(AppException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured while Getting Batch Upload Summary from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw new AppException("An error occured. Please, retry!.", 400);
                }
            }
        }

        public async Task<PagedData<BatchFileSummary>> GetUploadSummariesByUserId(string userId, SummaryPaginationFilter paginationFilter)
        {
            var result = new PagedData<BatchFileSummary>();

            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;
                try
                {
                   var results = await sqlConnection.QueryAsync<BatchFileSummary>(
                        sql: @"sp_get_batch_upload_summaries_by_user_id",
                        param: new
                        {
                            user_id = userId
                        },
                        commandType: CommandType.StoredProcedure);

                    //if (results == null)
                    //    throw new AppException($"No file has been uploaded by user!.", (int)HttpStatusCode.NotFound);

                    //filter by status
                    if (paginationFilter.Status == SummaryStatusEnum.Valid)
                    {
                        results = results.Where(e => e.NumOfRecords == e.NumOfValidRecords);
                    }

                    if(paginationFilter.Status == SummaryStatusEnum.Invalid)
                    {
                        results = results.Where(e => e.NumOfValidRecords == 0); 
                    }

                    if (paginationFilter.Status == SummaryStatusEnum.ValidAndInvalid)
                    {
                        results = results.Where(e => !(e.NumOfRecords == e.NumOfValidRecords) && !(e.NumOfValidRecords == 0));
                    }

                    //filter by productcode
                    IEnumerable<BatchFileSummary> resultList = new List<BatchFileSummary>();

                    if (!string.IsNullOrWhiteSpace(paginationFilter.ProductCode))
                            results = results
                                .Where(e => paginationFilter.ProductCode.ToLower().Equals(e.ProductCode, StringComparison.InvariantCultureIgnoreCase));

                    result.Data = results
                                    .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                    .Take(paginationFilter.PageSize);

                    result.TotalRowsCount = results.Count();

                    return result;
                }
                catch (AppException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured while Getting Batch Upload Summary from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw new AppException("An error occured. Please, retry!.", 400);
                }
            }
        }

        public async Task<long> GetBatchUploadSummaryId(string batchId)
        {
            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;

                try
                {
                    var batchSummaryId = await sqlConnection.QueryFirstOrDefaultAsync<long?>(
                        sql: @"sp_get_batch_upload_summary_id_by_batch_id",
                        param: new
                        {
                            batch_id = batchId
                        },
                        commandType: CommandType.StoredProcedure);

                    return batchSummaryId ?? throw new AppException($"Upload file with Batch Id: '{batchId}' not found!.", (int)HttpStatusCode.NotFound);
                }
                catch(AppException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured while performing Bill Payment Row Statuses operation from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw new AppException("An error occured. Please, retry!.", 400);
                }
            }
        }

        public async Task<IEnumerable<RowDetail>> GetConfirmedPayments(string batchId)
        {
            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;
                try
                {
                    var summary = await GetBatchUploadSummary(batchId);

                    if (!summary.TransactionStatus.ToLower().Equals(GenericConstants.AwaitingInitiation.ToLower()))
                        throw new AppException($"Payment cannot be initiated for upload with batchId:'{batchId}', because it is not in 'awaiting initiation' status!.");
                    
                    var summaryId = await GetBatchUploadSummaryId(batchId);

                    if (summaryId == 0)
                        throw new AppException($"Upload file with Batch Id: '{batchId}' not found!.", (int)HttpStatusCode.NotFound);

                    var result = await sqlConnection.QueryAsync<RowDetail>(
                        sql: GetSPForGetConfirmedPayments(summary.ItemType, summary.ContentType),
                        param: new
                        {
                            transactions_summary_id = summaryId,
                        },
                        commandType: CommandType.StoredProcedure);

                    return result ?? throw new AppException($"Upload file with Batch Id: '{batchId}' not found!.", (int)HttpStatusCode.NotFound);
                }
                catch (AppException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured while performing Bill Payment Row Statuses operation from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw new AppException("An error occured. Please, retry!.", 400);
                }
            }
        }

        private string GetSPForGetConfirmedPayments(string itemType, string contentType)
        {
            if (contentType.ToLower().Equals(GenericConstants.Firs) 
                && (itemType.ToLower().Equals(GenericConstants.BillPaymentId)
                || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem)))
            {
                return @"sp_get_confirmed_bill_payments_by_transactions_summary_id";
            }
            else if (contentType.ToLower().Equals(GenericConstants.Firs)
               && itemType.ToLower().Equals(GenericConstants.Wht))
            {
                return @"sp_get_confirmed_firs_wht_by_transactions_summary_id";
            }
            else if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.Wvat))
            {
                return @"sp_get_confirmed_firs_wvat_by_transactions_summary_id";
            }
            else if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.MultiTax))
            {
                return @"sp_get_confirmed_firs_multitax_by_transactions_summary_id";
            }
            else if (contentType.ToLower().Equals(GenericConstants.ManualCapture)
                && itemType.ToLower().Equals(GenericConstants.ManualCapture))
            {
                return @"sp_get_confirmed_fctirs_multitax_by_transactions_summary_id";
            }
            else if (contentType.ToLower().Equals(GenericConstants.Lasg)
                && itemType.ToLower().Equals(GenericConstants.Lasg))
            {
                return @"sp_get_confirmed_lasg_multitax_by_transactions_summary_id";
            }
            else return "";
        }

        public async Task<IEnumerable<RowDetail>> GetPaymentRowStatuses(string batchId, PaginationFilter pagination)
        {
            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;

                try
                {
                    var summaryId = await GetBatchUploadSummaryId(batchId);

                    if (summaryId == 0)
                        throw new AppException($"Upload file with Batch Id: '{batchId}' not found!.", (int)HttpStatusCode.NotFound);
                   
                    BatchFileSummary summary = await GetBatchUploadSummary(batchId);
                  
                    IEnumerable<RowDetail> result = new List<RowDetail>();

                   
                        result = await sqlConnection.QueryAsync<RowDetail>(
                          sql: GetSPForGetStatusBySummaryId(summary.ItemType, summary.ContentType),
                          param: GetStatusBySummaryIdParam(summary.ContentType, summary.ItemType, summaryId, 
                          pagination.PageSize, pagination.PageNumber, pagination.Status, pagination.TaxType),
                          commandType: CommandType.StoredProcedure);

                    return result;
                }
                catch(AppException ex)
                {
                    _logger.LogError("Error occured while performing Bill Payment Row Statuses operation from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured while performing Bill Payment Row Statuses operation from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw new AppException("An error occured. Please, retry!.", 400);
                }
            }
        }

        private object GetStatusBySummaryIdParam(string contentType, string itemType, long summaryId,
            int pageSize, int pageNumber, StatusEnum status, string taxType = "")
        {
            if (!(itemType.ToLower().Equals(GenericConstants.BillPaymentId)
               || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem))
               && contentType.ToLower().Equals(GenericConstants.BillPayment))

                return new
                {
                    transactions_summary_id = summaryId,
                    page_size = pageSize,
                    page_number = pageNumber,
                    status,
                    tax_type = taxType ?? GenericConstants.All
                };
            else
                return new
                {
                    transactions_summary_id = summaryId,
                    page_size = pageSize,
                    page_number = pageNumber,
                    status,
                };
        }

        private string GetSPForGetStatusBySummaryId(string itemType, string contentType)
        {
            if ((itemType.ToLower().Equals(GenericConstants.BillPaymentId)
                || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem)) 
                && contentType.ToLower().Equals(GenericConstants.BillPayment))
            {
                return @"sp_get_payments_status_by_transactions_summary_id";
            }
            else if(itemType.ToLower().Equals(GenericConstants.Wht)
                 && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_get_firs_wht_payments_status_by_transactions_summary_id";
            }
            else if (itemType.ToLower().Equals(GenericConstants.Wvat)
                 && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_get_firs_wvat_payments_status_by_transactions_summary_id";
            }
            else if (itemType.ToLower().Equals(GenericConstants.MultiTax)
                 && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_get_firs_multitax_payments_status_by_transactions_summary_id";
            }
            else if (itemType.ToLower().Equals(GenericConstants.ManualCapture)
                 && contentType.ToLower().Equals(GenericConstants.ManualCapture))
            {
                return @"sp_get_fctirs_multitax_payments_status_by_transactions_summary_id";
            }
            else if (itemType.ToLower().Equals(GenericConstants.Lasg)
                 && contentType.ToLower().Equals(GenericConstants.Lasg))
            {
                return @"sp_get_lasg_multitax_payments_status_by_transactions_summary_id";
            }
            else return "";
        }

        public async Task UpdateValidationResponse(UpdateValidationResponseModel updatePayments)
        {
            //get batchfilesummary from db 
            using (var connection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                connection.Open();

                try
                {
                    var fileSummary = await GetBatchUploadSummary(updatePayments.BatchId);

                    if (fileSummary == null)
                        throw new AppException($"Upload Batch Id '{updatePayments.BatchId}' not found!.", (int)HttpStatusCode.NotFound);

                    var rowsStatus = await GetPaymentRowStatuses(fileSummary.BatchId, 
                        new PaginationFilter 
                        { 
                            PageSize = fileSummary.NumOfRecords, 
                            PageNumber = 1, 
                            ItemType = fileSummary.ItemType 
                        });

                    var valids = updatePayments.RowStatuses
                        .Where(v => v.Status.ToLower()
                        .Equals("valid"));

                    decimal totalAmount = GenericHelpers.GetAmountSum(fileSummary.ContentType, fileSummary.ItemType, rowsStatus, valids);
                    decimal convenienceFee = 0;
                    if (valids.ToList().Count > 0)
                        convenienceFee = valids.FirstOrDefault().BatchConvenienceFee == 0 ? valids.Select(s => s.TransactionConvenienceFee).Sum() : valids.FirstOrDefault().BatchConvenienceFee;

                    using (var sqlTransaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var summaryId = await connection.ExecuteScalarAsync(
                                sql: "sp_update_bill_payment_upload_summary",
                                param: new
                                {
                                    batch_id = fileSummary.BatchId,
                                    num_of_valid_records = updatePayments.NumOfValidRecords,
                                    status = updatePayments.Status,
                                    modified_date = updatePayments.ModifiedDate,
                                    nas_tovalidate_file = updatePayments.NasToValidateFile,
                                    valid_amount_sum = totalAmount,
                                    convenienceFee
                                },
                            commandType: CommandType.StoredProcedure,
                            transaction: sqlTransaction); 

                            if(updatePayments.RowStatuses.Count() == 1)
                            {
                                RowValidationStatus status = updatePayments.RowStatuses.First();
                                await connection.ExecuteAsync(
                                        sql: GetSPToUpdateEnterpriseError(fileSummary.ItemType, fileSummary.ContentType) /*"sp_update_payments_detail_enterprise_error"*/,
                                        param:
                                            fileSummary.ContentType == GenericConstants.Lasg ?
                                        (object) new
                                        {
                                            transactions_summary_id = summaryId,
                                            surcharge = status.Surcharge,
                                            customerName = status.CustomerName,
                                            batchFee = status.BatchConvenienceFee,
                                            transactionFee = status.TransactionConvenienceFee,
                                            error = status.Error,
                                            row_status = status.Status,
                                            webguid = status.WebGuid
                                        } : 
                                        new
                                        {
                                            transactions_summary_id = summaryId,
                                            surcharge = status.Surcharge,
                                            customerName = status.CustomerName,
                                            batchFee = status.BatchConvenienceFee,
                                            transactionFee = status.TransactionConvenienceFee,
                                            error = status.Error,
                                            row_status = status.Status
                                        },
                                        commandType: CommandType.StoredProcedure,
                                        transaction: sqlTransaction);
                            }

                            if (updatePayments.RowStatuses.Count() > 1)
                            {
                                foreach (var status in updatePayments.RowStatuses)
                                {
                                    await connection.ExecuteAsync(
                                        sql: GetSPToUpdatePaymentDetail(fileSummary.ItemType, fileSummary.ContentType)/*"sp_update_payments_detail"*/,
                                        param: 
                                            fileSummary.ContentType == GenericConstants.Lasg ? 
                                                (object) new {
                                                    transactions_summary_id = summaryId,
                                                    error = status.Error,
                                                    row_num = status.Row,
                                                    surcharge = status.Surcharge,
                                                    batchFee = status.BatchConvenienceFee,
                                                    transactionFee = status.TransactionConvenienceFee,
                                                    customerName = status.CustomerName,
                                                    row_status = status.Status,
                                                    webguid = status.WebGuid
                                                } :
                                                new
                                                {
                                                    transactions_summary_id = summaryId,
                                                    error = status.Error,
                                                    row_num = status.Row,
                                                    surcharge = status.Surcharge,
                                                    customerName = status.CustomerName,
                                                    batchFee = status.BatchConvenienceFee,
                                                    transactionFee = status.TransactionConvenienceFee,
                                                    row_status = status.Status
                                                },
                                        commandType: CommandType.StoredProcedure,
                                        transaction: sqlTransaction);
                                }
                            }
                            sqlTransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            sqlTransaction.Rollback();
                            throw ex;
                        }
                    }
                }
                catch(AppException ex)
                {
                    throw ex;
                }    
                catch (Exception ex)
                {
                    _logger.LogError("Error occured while performing Update Bill Payment Validation operation from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw new AppException("An error occured. Please, retry!.", 400);
                }
            }
        }

        private string GetSPToUpdatePaymentDetail(string itemType, string contentType)
        {
            if (itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
            {
                return @"sp_update_bill_payments_detail";
            }
            else if (itemType.ToLower().Equals(GenericConstants.Wht.ToLower())
                && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_update_firs_wht_payments_detail";
            }
            else if (itemType.ToLower().Equals(GenericConstants.Wvat.ToLower())
                 && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_update_firs_wvat_payments_detail";
            }
            else if (itemType.ToLower().Equals(GenericConstants.MultiTax)
                 && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_update_firs_multitax_payments_detail";
            }
            else if (itemType.ToLower().Equals(GenericConstants.ManualCapture)
                 && contentType.ToLower().Equals(GenericConstants.ManualCapture))
            {
                return @"sp_update_fctirs_multitax_payments_detail";
            }
            else if (itemType.ToLower().Equals(GenericConstants.Lasg)
                 && contentType.ToLower().Equals(GenericConstants.Lasg))
            {
                return @"sp_update_lasg_multitax_payments_detail";
            }
            else return "";
        }

        private string GetSPToUpdateEnterpriseError(string itemType, string contentType)
        {
            if ((itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
               || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
               && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_update_bill_payments_detail_enterprise_error";
            }
            else if (itemType.ToLower().Equals(GenericConstants.Wht.ToLower()) 
                && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_update_firs_wht_detail_enterprise_error";
            }
            else if (itemType.ToLower().Equals(GenericConstants.Wvat.ToLower())
                && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_update_firs_wvat_detail_enterprise_error";
            }
            else if(itemType.ToLower().Equals(GenericConstants.MultiTax)
                && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                return @"sp_update_firs_multitax_detail_enterprise_error";
            }
            else if (itemType.ToLower().Equals(GenericConstants.ManualCapture)
                && contentType.ToLower().Equals(GenericConstants.ManualCapture))
            {
                return @"sp_update_fctirs_multitax_detail_enterprise_error";
            }
            else if (itemType.ToLower().Equals(GenericConstants.Lasg)
                && contentType.ToLower().Equals(GenericConstants.Lasg))
            {
                return @"sp_update_lasg_multitax_detail_enterprise_error";
            }
            else return "";
        }

        public async Task UpdatePaymentInitiation(string batchId)
        {
            using (var connection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                connection.Open();

                try
                {
                    var fileSummary = await GetBatchUploadSummary(batchId);

                    if (fileSummary == null)
                        throw new AppException($"Upload with Batch Id: {batchId} not found!.", (int)HttpStatusCode.NotFound);

                    await connection.ExecuteAsync(
                        sql: "sp_update_bill_payment_summary_status",
                        param: new
                        {
                            batch_id = fileSummary.BatchId,
                            status = GenericConstants.PaymentInitiated,
                            modified_date = DateTime.Now.ToString()
                        },
                        commandType: CommandType.StoredProcedure);
                }
                catch (AppException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured while performing Update Bill Payment Initiation operation from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw new AppException("An error occured. Please, retry!.", 400);
                }
            }
        }
    }
}
