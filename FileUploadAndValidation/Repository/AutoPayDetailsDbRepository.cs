using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using FileUploadApi;
using Dapper;
using System;
using FilleUploadCore.Exceptions;
using System.Net;
using FileUploadAndValidation.Utils;
using Microsoft.Extensions.Logging;
using FileUploadAndValidation.Models;

namespace FileUploadAndValidation.Repository
{
    public class AutoPayDetailsDbRepository : IDetailsDbRepository<AutoPayRow>
    {
        private readonly IAppConfig _appConfig;
        private readonly ILogger<AutoPayDetailsDbRepository> _logger;
        private readonly BatchFileSummaryDbRepository _batchRepository;

        public AutoPayDetailsDbRepository(BatchFileSummaryDbRepository batchRepository, 
                                          IAppConfig appConfig, 
                                          ILogger<AutoPayDetailsDbRepository> logger)
        {
            _appConfig = appConfig;
            _logger = logger;        
            _batchRepository = batchRepository;
        }
        public async Task<string> InsertAllUploadRecords(Batch<AutoPayRow> batch)
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
                            var batchSummary = this.FromBatch(batch);
                            var batchId = await _batchRepository.CreateBatchSummary((connection, sqlTransaction), batchSummary);

                            foreach (var row in batch.Rows)
                            {
                                await connection.ExecuteAsync(sql: "sp_insert_valid_bill_payments",
                                    param: new
                                    {
                                        product_code = "0",//row.ProductCode,
                                        item_code =  "0",//.ItemCode,
                                        customer_id =  "0",//row.CustomerId,
                                        amount = row.Amount,
                                        row_status = "Invalid",
                                        created_date =  DateTime.Now,//row.CreatedDate,
                                        row_num =  "0",//row.RowNumber,
                                        transactions_summary_Id = batchId,
                                        initial_validation_status = "Invalid",
                                        error =  "0",//row.Error
                                    },
                                    transaction: sqlTransaction,
                                    commandType: System.Data.CommandType.StoredProcedure);
                            };

                            sqlTransaction.Commit();
                            return batch.BatchId;
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

        private UploadSummaryDto FromBatch(Batch<AutoPayRow> batch){
            return new UploadSummaryDto
            {
                BatchId = batch.BatchId,
                NumOfAllRecords = batch.NumOfRecords,
                Status = batch.TransactionStatus,
                UploadDate = batch.UploadDate,
                CustomerFileName = "",
                ItemType = batch.ItemType,
                ContentType = batch.ContentType,
                UserId = batch.UserId, //
                ProductName = batch.ProductName,
                ProductCode = batch.ProductCode
            };

        }
    }
}