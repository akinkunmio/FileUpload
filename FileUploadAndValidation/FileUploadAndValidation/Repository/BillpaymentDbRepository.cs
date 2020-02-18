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

namespace FileUploadAndValidation.Repository
{
    public class BillPaymentRepository : IBillPaymentDbRepository
    {
        private readonly IAppConfig _appConfig;

        public BillPaymentRepository(IAppConfig appConfig)
        {
            _appConfig = appConfig;
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
                            //use automapper to map objects
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
            catch (Exception)
            {
                throw new AppException("An error occured while connecting to database!.", 500);
            }
        }

        public async Task<BatchFileSummary> GetBatchUploadSummary(string batchId)
        {

            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;
                var batchFileSummary = await sqlConnection.QueryFirstOrDefaultAsync<BatchFileSummary>(
                    sql: @"sp_get_batch_upload_summary_by_batch_id",
                    param: new 
                    { 
                        batch_id = batchId
                    }, 
                    commandType: CommandType.StoredProcedure);

                return batchFileSummary;
            }
        }

        public async Task<long> GetBatchUploadSummaryId(string batchId)
        {
            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;

                var batchSummaryId = await sqlConnection.QueryFirstOrDefaultAsync<long>(
                    sql: @"sp_get_batch_upload_summary_id_by_batch_id",
                    param: new
                    {
                        batch_id = batchId
                    },
                    commandType: CommandType.StoredProcedure);

                return batchSummaryId;
            }
        }

        public async Task<IEnumerable<BillPayment>> GetBillPayments(long id)
        {
                using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
                {
                    MatchNamesWithUnderscores = true;

                    var result = await sqlConnection.QueryAsync<BillPayment>(
                        sql: @"sp_get_bill_payments",
                        param: new 
                        {
                            transactions_summary_id = id
                        }, 
                        commandType: CommandType.StoredProcedure);

                    return result;
                }
        }

        public async Task<IEnumerable<BillPaymentRowStatus>> GetBillPaymentRowStatuses(string batchId)
        {
            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                MatchNamesWithUnderscores = true;

                var summaryId = await GetBatchUploadSummaryId(batchId);
                var result = await sqlConnection.QueryAsync<BillPaymentRowStatus>(
                    sql: @"sp_get_bill_payments_status_by_batch_id",
                    param: new
                    {
                        transactions_summary_id = summaryId
                    },
                    commandType: CommandType.StoredProcedure);

                return result;
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
                        throw new AppException($"Upload with Batch Id {updateBillPayments.BatchId} not found!.");
                    using (var sqlTransaction = connection.BeginTransaction())
                    {
                        try
                        {


                            var summaryId =  await connection.ExecuteScalarAsync(
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

                            foreach(var status in updateBillPayments.RowStatuses)
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
                        catch (Exception)
                        {
                            sqlTransaction.Rollback();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new AppException("An error occured while updating validation to DB", (int)HttpStatusCode.InternalServerError);
                }
            }
            //if does not exist throw an error
            //then 
            //update batchfilesummary and all transactions
        }
    }
}
