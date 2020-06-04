using System.Collections.Generic;
using System.Threading.Tasks;
using FileUploadApi;
using FileUploadAndValidation.Utils;
using Microsoft.Extensions.Logging;
using FileUploadAndValidation.BillPayments;
using FilleUploadCore.Exceptions;
using System;
using System.Net;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using FileUploadAndValidation.Models;

namespace FileUploadAndValidation.Repository
{
    public class ManualCaptureDbRepository : IDetailsDbRepository<ManualCaptureRow>
    {
        private readonly IAppConfig _appConfig;
        private readonly ILogger<AutoPayDetailsDbRepository> _logger;
        private readonly BatchFileSummaryDbRepository _batchRepository;

        public ManualCaptureDbRepository(BatchFileSummaryDbRepository batchRepository,
                                          IAppConfig appConfig,
                                          ILogger<AutoPayDetailsDbRepository> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
            _batchRepository = batchRepository;
        }

        public async Task<string> InsertAllUploadRecords(Batch<ManualCaptureRow> batch)
        {
            try
            {
                using (var connection = new SqlConnection(_appConfig.UploadServiceConnectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var batchSummary = FromBatch(batch);
                            var transactionSummaryId = await _batchRepository.CreateBatchSummary((connection, transaction), batchSummary);

                            foreach (var row in batch.Rows)
                            {
                                await connection.ExecuteAsync(sql: "sp_insert_invalid_fctirs_multitax",
                                    param: new {
                                        product_code = row.ProductCode,
                                        item_code = row.ItemCode,
                                        customer_id = row.CustomerId,
                                        amount = row.Amount,
                                        desc = row.Description,
                                        customer_name = row.CustomerName,
                                        phone_number = row.PhoneNumber,
                                        email = row.Email,
                                        address = row.Address,
                                        transactions_summary_id = transactionSummaryId,
                                        row_num = row.Index,
                                        row_status = row.IsValid ? "Valid" : "Invalid",
                                        created_date = batch.UploadDate,
                                        initial_validation_status = "validation-in-progress",
                                        error = row.ErrorMessages?.ToString(),                                      
                                        },
                                    transaction: transaction,
                                    commandType: System.Data.CommandType.StoredProcedure);
                            }
                            
                            transaction.Commit();
                            return batch.BatchId;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while inserting payment items in database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException("An error occured while querying the DB");
            }

        }


        private UploadSummaryDto FromBatch(Batch<ManualCaptureRow> batch){
            return new UploadSummaryDto
            {
                BatchId = batch.BatchId,
                UserId = batch.UserId,
                NumOfAllRecords = batch.NumOfRecords,               
                Status = batch.TransactionStatus,
                UploadDate = batch.UploadDate,
                CustomerFileName = "",
                ItemType = batch.ItemType,                
                ContentType = batch.ContentType,
                ProductName = batch.ProductName,
                ProductCode = batch.ProductCode
            };

        }
    }
}