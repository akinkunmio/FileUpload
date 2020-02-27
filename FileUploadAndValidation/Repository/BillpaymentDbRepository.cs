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

        public async Task<string> InsertPaymentUpload(UploadSummaryDto fileDetail, List<BillPayment> billPayments)
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
                                   content_type = fileDetail.ContentType
                               },
                               transaction: sqlTransaction,
                               commandType: System.Data.CommandType.StoredProcedure);


                            foreach (var billPayment in billPayments)
                            {
                                await connection.ExecuteAsync(sql: "sp_insert_bill_payments",
                                    param: new
                                    {
                                        product_code = billPayment.ProductCode,
                                        item_code = billPayment.ItemCode,
                                        customer_id = billPayment.CustomerId,
                                        amount = billPayment.Amount,
                                        row_status = billPayment.Status,
                                        created_date = billPayment.CreatedDate,
                                        row_num = billPayment.RowNumber,
                                        transactions_summary_Id = transactionSummaryId
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

                    return batchFileSummary;
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
                    var batchSummaryId = await sqlConnection.QueryFirstOrDefaultAsync<long>(
                        sql: @"sp_get_batch_upload_summary_id_by_batch_id",
                        param: new
                        {
                            batch_id = batchId
                        },
                        commandType: CommandType.StoredProcedure);

                    return batchSummaryId;
                }
                catch (Exception ex)
                {
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
                        throw new AppException($"Upload file with Batch Id: '{batchId}' not found!.");

                    var result = await sqlConnection.QueryAsync<ConfirmedBillPaymentDto>(
                        sql: @"sp_get_confirmed_bill_payments_by_transactions_summary_id",
                        param: new
                        {
                            transactions_summary_id = summaryId,
                        },
                        commandType: CommandType.StoredProcedure);

                    return result;
                }
                catch (AppException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new AppException("An error occured while querying the DB", (int)HttpStatusCode.InternalServerError);
                }
            }
        }

        public async Task<IEnumerable<BillPaymentRowStatusDto>> GetBillPaymentRowStatuses(string batchId, PaginationFilter pagination)
        {
            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;

                try
                {
                    var summaryId = await GetBatchUploadSummaryId(batchId);

                    if (summaryId == 0)
                        throw new AppException($"Upload file with Batch Id: '{batchId}' not found!.");

                    var result = await sqlConnection.QueryAsync<BillPaymentRowStatusDto>(
                        sql: @"sp_get_bill_payments_status_by_transactions_summary_id",
                        param: new
                        {
                            transactions_summary_id = summaryId,
                            page_size = pagination.PageSize,
                            page_number = pagination.PageNumber
                        },
                        commandType: CommandType.StoredProcedure);

                    return result;
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
                    var fileSummary = await GetBatchUploadSummary(updateBillPayments.BatchId);

                    if (fileSummary == null)
                        throw new AppException($"Upload Batch Id '{updateBillPayments.BatchId}' not found!.");

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
                                    nas_tovalidate_file = updateBillPayments.NasToValidateFile
                                },
                            commandType: CommandType.StoredProcedure,
                            transaction: sqlTransaction);

                            foreach (var status in updateBillPayments.RowStatuses)
                            {

                                await connection.ExecuteAsync(
                                    sql: "sp_update_bill_payments",
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
                catch (Exception)
                {
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
                        throw new AppException($"Upload with Batch Id: {batchId} not found!.");

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
