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

namespace FileUploadAndValidation.Repository
{
    public class BillPaymentRepository : IBillPaymentDbRepository
    {
        private readonly IAppConfig _appConfig;
        public BillPaymentRepository(IAppConfig appConfig)
        {
            _appConfig = appConfig;
        }
        public async Task<string> CreateBatchPaymentUpload(BatchFileSummary fileDetail, List<BillPayment> billPayments)
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
                            var transactionSummaryId = await connection.ExecuteScalarAsync(sql: "sp_add_bill_payment_batch_transaction_summary",
                               param: new
                               {
                                   batch_id = fileDetail.BatchId,
                                   status = fileDetail.Status,
                                   item_type = fileDetail.ItemType,
                                   num_of_records = fileDetail.NumOfAllRecords,
                                   upload_date = fileDetail.UploadDate,
                                   uploaded_by = fileDetail.UploadedBy,
                                   num_of_valid_records = fileDetail.NumOfValidRecords,
                                   content_type = fileDetail.ContentType,
                               },
                               transaction: sqlTransaction,
                               commandType: System.Data.CommandType.StoredProcedure);


                            foreach (var billPayment in billPayments)
                            {
                                await connection.ExecuteAsync(sql: "sp_add_billpayments",
                                    param: new
                                    {
                                        product_code = billPayment.ProductCode,
                                        item_code = billPayment.ItemCode,
                                        customer_id = billPayment.CustomerId,
                                        amount = billPayment.Amount,
                                        batch_id = billPayment.BatchId,
                                        ent_error_response = billPayment.EnterpriseErrorResponse,
                                        ent_reference_id = billPayment.EnterpriseReferenceId,
                                        status = billPayment.Status,
                                        created_date = billPayment.CreatedDate,
                                        modified_date = billPayment.ModifiedDate,
                                        row_number = billPayment.RowNumber,
                                        transactions_summary_Id = transactionSummaryId
                                    });
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
            catch (Exception exception) 
            {
                throw exception;
            }
        }

        public async Task<BatchFileSummary> GetBatchUploadSummary(string batchId, string userName)
        {
            using (var sqlConnection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                var batchFileSummary = await sqlConnection.QueryFirstOrDefaultAsync<BatchFileSummary>(
                    sql: @"sp_get_batch_upload_summary",
                    param: new 
                    { 
                        batchId, 
                        userName 
                    }, 
                    commandType: CommandType.StoredProcedure);

                return batchFileSummary;
            }
        }

        public async Task<IEnumerable<BillPayment>> GetBillPayments(long id)
        {
                using (var conn = new SqlConnection(_appConfig.UploadServiceConnectionString))
                {
                    var result = await conn.QueryAsync<BillPayment>(
                        sql: @"sp_get_bill_payments",
                        param: new 
                        { 
                            id 
                        }, 
                        commandType: CommandType.StoredProcedure);

                    return result;
                }
        }

        public Task<IEnumerable<BillPayment>> GetBillPayments(string batchId, string userName)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateBatchUpload(UpdateBillPaymentsCollection updateBillPayments)
        {
            //get batchfilesummary from db 
            using (var connection = new SqlConnection(_appConfig.UploadServiceConnectionString))
            {
                connection.Open();
                try
                {
                    BatchFileSummary fileSummary = await GetBatchUploadSummary(updateBillPayments.BatchId, updateBillPayments.UserName);
                    if (fileSummary == null)
                        throw new AppException($"Upload with Batch Id {updateBillPayments.BatchId} not found!.");
                    using (var sqlTransaction = connection.BeginTransaction())
                    {
                        try
                        {
                            await connection.ExecuteAsync(
                                sql: "sp_update_bill_payment_upload_summary",
                                param: new
                                {
                                    id = fileSummary.Id,
                                    user_name = updateBillPayments.UserName,
                                    batch_id = updateBillPayments.BatchId,
                                    status = updateBillPayments.Status,
                                    modified_date = updateBillPayments.ModifiedDate,
                                    nas_validated_file_name = updateBillPayments.NasValidatedFileName,
                                    nas_confirmed_file_name = updateBillPayments.NasConfirmedFileName,
                                },
                            commandType: CommandType.StoredProcedure,
                            transaction: sqlTransaction);

                            foreach(var billPayment in updateBillPayments.BillPayments)
                            {

                                await connection.ExecuteAsync(
                                    sql: "sp_update_bill_payment",
                                    param: new
                                    {
                                        modified_date = billPayment.ModifiedDate,
                                        status = billPayment.Status,
                                        transactions_summary_id = fileSummary.Id
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
                catch (Exception)
                {

                }
            }
            //if does not exist throw an error
            //then 
            //update batchfilesummary and all transactions
        }
    }
}
