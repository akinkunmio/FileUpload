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
        private readonly ILogger<IDbRepository> _logger;

        public DbRepository(IAppConfig appConfig, ILogger<IDbRepository> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
        }
        
        public async Task<string> InsertAllUploadRecords(UploadSummaryDto fileDetail, IList<RowDetail> payments, IList<Failure> invalidPayments, string itemType = null)
        {
            try
            {
                using (var connection = new SqlConnection(_appConfig.UploadServiceConnectionString))
                {
                    connection.Open();

                    using (var sqlTransaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var transactionSummaryId = await connection.ExecuteScalarAsync(sql: "sp_insert_bill_payment_transaction_summary",
                               param: new
                               {
                                   batch_id = fileDetail.BatchId,
                                   status = fileDetail.Status,
                                   item_type = fileDetail.ItemType,
                                   num_of_records = fileDetail.NumOfAllRecords,
                                   upload_date = fileDetail.UploadDate,
                                   content_type = fileDetail.ContentType,
                                   nas_raw_file = fileDetail.NasRawFile,
                                   userid = fileDetail.UserId
                               },
                               transaction: sqlTransaction,
                               commandType: System.Data.CommandType.StoredProcedure);

                            if (itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()) 
                                || itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower()))
                            {
                                foreach (var billPayment in payments)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_valid_bill_payments",
                                        param: new
                                        {
                                            product_code = billPayment.ProductCode,
                                            item_code = billPayment.ItemCode,
                                            customer_id = billPayment.CustomerId,
                                            amount = billPayment.Amount,
                                            created_date = billPayment.CreatedDate,
                                            row_num = billPayment.RowNumber,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Valid"
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }

                                foreach (var invalid in invalidPayments)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_invalid_bill_payments",
                                        param: new
                                        {
                                            product_code = invalid.Row.ProductCode,
                                            item_code = invalid.Row.ItemCode,
                                            customer_id = invalid.Row.CustomerId,
                                            amount = invalid.Row.Amount,
                                            row_num = invalid.Row.RowNumber,
                                            row_status = "Invalid",
                                            created_date = invalid.Row.CreatedDate,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Invalid",
                                            error = invalid.Row.Error
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }
                            }

                            if (itemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                            {
                                foreach (var firsWht in payments)
                                {
                                    //create sp sp_insert_valid_firs_wht
                                    await connection.ExecuteAsync(sql: "sp_insert_valid_firs_wht",
                                        param: new
                                        {
                                            beneficiary_address = firsWht.BeneficiaryAddress,
                                            beneficiary_name = firsWht.BeneficiaryName,
                                            beneficiary_tin = firsWht.BeneficiaryTin,
                                            contract_amount = firsWht.ContractAmount,
                                            contract_date = firsWht.ContractDate,
                                            contract_type = firsWht.ContractType,
                                            wht_rate = firsWht.WhtRate,
                                            wht_amount = firsWht.WhtAmount,
                                            period_covered = firsWht.PeriodCovered,
                                            invoice_number = firsWht.InvoiceNumber,
                                            created_date = firsWht.CreatedDate,
                                            row_num = firsWht.RowNumber,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Valid"
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }

                                foreach (var invalid in invalidPayments)
                                {
                                    //create sp sp_insert_invalid_firs_wht
                                    await connection.ExecuteAsync(sql: "sp_insert_invalid_firs_wht",
                                        param: new
                                        {
                                            beneficiary_address = invalid.Row.BeneficiaryAddress,
                                            beneficiary_name = invalid.Row.BeneficiaryName,
                                            beneficiary_tin = invalid.Row.BeneficiaryTin,
                                            contract_amount = invalid.Row.ContractAmount,
                                            contract_date = invalid.Row.ContractDate,
                                            contract_type = invalid.Row.ContractType,
                                            wht_rate = invalid.Row.WhtRate,
                                            wht_amount = invalid.Row.WhtAmount,
                                            period_covered = invalid.Row.PeriodCovered,
                                            invoice_number = invalid.Row.InvoiceNumber,
                                            row_status = "Invalid",
                                            created_date = invalid.Row.CreatedDate,
                                            row_num = invalid.Row.RowNumber,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Invalid",
                                            error = invalid.Row.Error
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }
                            }

                            if (itemType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                            {
                                foreach (var validWht in payments)
                                {
                                    //create sp sp_insert_valid_firs_wvat
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
                                            row_num = validWht.RowNumber,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Valid"
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }

                                foreach (var invalidWvat in invalidPayments)
                                {
                                    // create sp_insert_invalid_firs_wvat
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
                                            row_num = invalidWvat.Row.RowNumber,
                                            transactions_summary_Id = transactionSummaryId,
                                            row_status = "Invalid",
                                            initial_validation_status = "Invalid",
                                            error = invalidWvat.Row.Error
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
                throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
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
                    throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
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
                    throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
                }
            }
        }

        public async Task<PagedData<BatchFileSummary>> GetUploadSummariesByUserId(string userId, PaginationFilter paginationFilter)
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

                    if (results == null)
                        throw new AppException($"No file has been uploaded by user!.", (int)HttpStatusCode.NotFound);

                    result.Data = results.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize).Take(paginationFilter.PageSize);

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
                    throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
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
                    throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
                }
            }
        }

        public async Task<IEnumerable<RowDetail>> GetConfirmedBillPayments(string batchId)
        {
            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;
                try
                {
                    var summary = await GetBatchUploadSummary(batchId);

                    if (!summary.TransactionStatus.ToLower().Equals(GenericConstants.AwaitingInitiation.ToLower()))
                        throw new AppException($"Upload Batch Id: '{batchId}' is not in awaiting initiation status!.");
                    
                    var summaryId = await GetBatchUploadSummaryId(batchId);

                    if (summaryId == 0)
                        throw new AppException($"Upload file with Batch Id: '{batchId}' not found!.", (int)HttpStatusCode.NotFound);

                    var result = await sqlConnection.QueryAsync<RowDetail>(
                        sql: GetSPForGetConfirmedPayments(summary.ItemType),
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
                    throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
                }
            }
        }

        private string GetSPForGetConfirmedPayments(string itemType)
        {
            if (itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
            {
                return @"sp_get_confirmed_bill_payments_by_transactions_summary_id";
            }
            else if (itemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
            {
                return @"sp_get_confirmed_firs_wht_by_transactions_summary_id";
            }
            else if (itemType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
            {
                return @"sp_get_confirmed_firs_wvat_by_transactions_summary_id";
            }
            else return "";
            
        }

        public async Task<RowStatusDtoObject> GetPaymentRowStatuses(string batchId, PaginationFilter pagination)
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
                       sql: GetSPForGetStatusBySummaryId(pagination.ItemType),
                       param: new
                       {
                           transactions_summary_id = summaryId,
                           page_size = pagination.PageSize,
                           page_number = pagination.PageNumber,
                           status = pagination.Status
                       },
                       commandType: CommandType.StoredProcedure);

                    if (result == null)
                        throw new AppException($"No records found for file with batchId '{batchId}'");

                    return new RowStatusDtoObject 
                    { 
                        RowStatusDto = result, 
                        TotalRowsCount = summary.NumOfRecords, 
                        InvalidRowCount = summary.NumOfRecords - summary.NumOfValidRecords, 
                        ValidRowCount = summary.NumOfValidRecords, 
                        ValidAmountSum = summary.ValidAmountSum 
                    };
                }
                catch(AppException ex)
                {
                    _logger.LogError("Error occured while performing Bill Payment Row Statuses operation from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured while performing Bill Payment Row Statuses operation from database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                    throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
                }
            }
        }

        private string GetSPForGetStatusBySummaryId(string itemType)
        {
            if (itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
            {
                return @"sp_get_bill_payments_status_by_transactions_summary_id";
            }
            else if(itemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
            {
                return @"sp_get_firs_wht_payments_status_by_transactions_summary_id";
            }
            else if (itemType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
            {
                return @"sp_get_firs_wvat_payments_status_by_transactions_summary_id";
            }
            else return "";
        }

        public async Task UpdateValidationResponse(UpdateValidationResponseModel updateBillPayments)
        {
            //get batchfilesummary from db 
            using (var connection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                connection.Open();

                try
                {
                    var fileSummary = await GetBatchUploadSummary(updateBillPayments.BatchId);

                    if (fileSummary == null)
                        throw new AppException($"Upload Batch Id '{updateBillPayments.BatchId}' not found!.", (int)HttpStatusCode.NotFound);

                    var rowStatusDto = await GetPaymentRowStatuses(fileSummary.BatchId, 
                        new PaginationFilter 
                        { 
                            PageSize = fileSummary.NumOfRecords, 
                            PageNumber = 1, 
                            ItemType = fileSummary.ItemType 
                        });

                    var valids = updateBillPayments.RowStatuses?.Where(v => v.Status.ToLower().Equals("valid"));
                    var totalAmount = rowStatusDto.RowStatusDto?.Where(r => valids.Any(v => v.Row == r.RowNumber)).Select(s => decimal.Parse(s.Amount)).Sum();

                    using (var sqlTransaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var summaryId = await connection.ExecuteScalarAsync(
                                sql: "sp_update_bill_payment_upload_summary",
                                param: new
                                {
                                    batch_id = fileSummary.BatchId,
                                    num_of_valid_records = updateBillPayments.NumOfValidRecords,
                                    status = updateBillPayments.Status,
                                    modified_date = updateBillPayments.ModifiedDate,
                                    nas_tovalidate_file = updateBillPayments.NasToValidateFile,
                                    valid_amount_sum = totalAmount
                                },
                            commandType: CommandType.StoredProcedure,
                            transaction: sqlTransaction); 

                            if(updateBillPayments.RowStatuses.Count() == 1)
                            {
                                RowValidationStatus status = updateBillPayments.RowStatuses.First();
                                await connection.ExecuteAsync(
                                        sql: GetSPToUpdateEnterpriseError(fileSummary.ItemType) /*"sp_update_bill_payments_detail_enterprise_error"*/,
                                        param: new
                                        {
                                            transactions_summary_id = summaryId,
                                            error = status.Error,
                                            row_status = status.Status
                                        },
                                        commandType: CommandType.StoredProcedure,
                                        transaction: sqlTransaction);
                            }

                            if (updateBillPayments.RowStatuses.Count() > 1)
                            {
                                foreach (var status in updateBillPayments.RowStatuses)
                                {
                                    await connection.ExecuteAsync(
                                        sql: GetSPToUpdatePaymentDetail(fileSummary.ItemType)/*"sp_update_bill_payments_detail"*/,
                                        param: new
                                        {
                                            transactions_summary_id = summaryId,
                                            error = status.Error,
                                            row_num = status.Row,
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
                    throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
                }
            }
        }

        private string GetSPToUpdatePaymentDetail(string itemType)
        {
            if (itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
            {
                return @"sp_update_bill_payments_detail";
            }
            else if (itemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
            {
                return @"sp_update_wht_payments_detail";
            }
            else if (itemType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
            {
                return @"sp_update_wvat_payments_detail";
            }
            else return "";
        }

        private string GetSPToUpdateEnterpriseError(string itemType)
        {
            if (itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
               || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()))
            {
                return @"sp_update_bill_payments_detail_enterprise_error";
            }
            else if (itemType.ToLower().Equals(GenericConstants.WHT.ToLower()))
            {
                return @"sp_update_wht_payments_detail_enterprise_error";
            }
            else if (itemType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
            {
                return @"sp_update_wvat_payments_detail_enterprise_error";
            }
            else return "";
        }

      
        public async Task UpdateBillPaymentInitiation(string batchId)
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
                            status = GenericConstants.AwaitingApproval,
                            modified_date = DateTime.Now.ToString(),
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
                    throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
                }
            }
        }

    }
}
