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
    public class BillPaymentRepository : IBillPaymentDbRepository
    {
        private readonly IAppConfig _appConfig;
        private readonly ILogger<BillPaymentRepository> _logger;

        public BillPaymentRepository(IAppConfig appConfig, ILogger<BillPaymentRepository> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
        }
        
        public async Task<string> InsertAllUploadRecords(UploadSummaryDto fileDetail, List<BillPayment> billPayments, List<FailedBillPayment> invalidBillPayments)
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

                            foreach (var billPayment in billPayments)
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

                            foreach (var billPayment in invalidBillPayments)
                            {
                                await connection.ExecuteAsync(sql: "sp_insert_invalid_bill_payments",
                                    param: new
                                    {
                                        product_code = billPayment.ProductCode,
                                        item_code = billPayment.ItemCode,
                                        customer_id = billPayment.CustomerId,
                                        amount = billPayment.Amount,
                                        row_status = "Invalid",
                                        created_date = billPayment.CreatedDate,
                                        row_num = billPayment.RowNumber,
                                        transactions_summary_Id = transactionSummaryId,
                                        initial_validation_status = "Invalid",
                                        error = billPayment.Error
                                    },
                                    transaction: sqlTransaction,
                                    commandType: System.Data.CommandType.StoredProcedure);
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

        public async Task<IEnumerable<ConfirmedBillPaymentDto>> GetConfirmedBillPayments(string batchId)
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

                    var result = await sqlConnection.QueryAsync<ConfirmedBillPaymentDto>(
                        sql: @"sp_get_confirmed_bill_payments_by_transactions_summary_id",
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

        public async Task<BillPaymentRowStatusDtoObject> GetBillPaymentRowStatuses(string batchId, PaginationFilter pagination)
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
                    var totalRowsCount = summary.NumOfRecords;

                    var result = await sqlConnection.QueryAsync<BillPaymentRowStatusDto>(
                        sql: @"sp_get_bill_payments_status_by_transactions_summary_id",
                        param: new
                        {
                            transactions_summary_id = summaryId,
                            page_size = pagination.PageSize,
                            page_number = pagination.PageNumber
                        },
                        commandType: CommandType.StoredProcedure);

                    if (result == null)
                        throw new AppException($"No records was found for the file with batchId '{batchId}'");

                    return new BillPaymentRowStatusDtoObject { RowStatusDtos = result, TotalRowsCount = totalRowsCount, ValidAmountSum = summary.ValidAmountSum };
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

        public async Task UpdateValidationResponse(UpdateValidationResponseModel updateBillPayments)
        {
            //get batchfilesummary from db 
            using (var connection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                connection.Open();

                try
                {
                    BatchFileSummary fileSummary = await GetBatchUploadSummary(updateBillPayments.BatchId);

                    if (fileSummary == null)
                        throw new AppException($"Upload Batch Id '{updateBillPayments.BatchId}' not found!.", (int)HttpStatusCode.NotFound);

                    var rowStatusDto = await GetBillPaymentRowStatuses(fileSummary.BatchId, new PaginationFilter (fileSummary.NumOfRecords, 1 ));

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
                                    valid_amount_sum = rowStatusDto?.RowStatusDtos.Where(v => v.RowStatus.ToLower().Equals("valid"))?.Select(s => s.Amount).Sum()
                                },
                            commandType: CommandType.StoredProcedure,
                            transaction: sqlTransaction); 

                            if(updateBillPayments.RowStatuses.Count() == 1)
                            {
                                var status = updateBillPayments.RowStatuses.First();
                                await connection.ExecuteAsync(
                                        sql: "sp_update_bill_payments_detail_enterprise_error",
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
                                        sql: "sp_update_bill_payments_detail",
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
