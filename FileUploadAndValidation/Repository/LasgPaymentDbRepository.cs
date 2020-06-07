using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using FileUploadAndValidation.BillPayments;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Utils;
using FileUploadApi;
using FilleUploadCore.Exceptions;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Collections.Generic;

namespace FileUploadAndValidation.Repository
{
    public class LasgPaymentDbRepository : IDetailsDbRepository<LASGPaymentRow>
    {
        private readonly IAppConfig _appConfig;
        private readonly ILogger<LasgPaymentDbRepository> _logger;
        private readonly BatchFileSummaryDbRepository _batchRepository;
        public LasgPaymentDbRepository(BatchFileSummaryDbRepository batchRepository,
                                          IAppConfig appConfig,
                                          ILogger<LasgPaymentDbRepository> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
            _batchRepository = batchRepository;        
        }

        public async Task<string> InsertAllUploadRecords(Batch<LASGPaymentRow> batch)
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
                                await connection.ExecuteAsync(sql: "sp_insert_invalid_lirs_multitax",
                                    param: new {
                                        product_code = row.ProductCode,
                                        item_code = row.ItemCode,
                                        customer_id = row.CustomerId,
                                        payer_id = row.PayerId,
                                        agency_code = row.AgencyCode,
                                        revenue_code = row.RevenueCode,
                                        start_period = row.StartPeriod,
                                        end_period = row.EndPeriod,
                                        narration = row.Description,
                                        amount = row.Amount,
                                        tax_type = "",
                                        customer_name = "",
                                        row_status = row.IsValid ? "Valid" : "Invalid",
                                        created_date = batch.UploadDate,
                                        modified_date = batch.UploadDate,
                                        row_num = row.Row,
                                        transactions_summary_id = transactionSummaryId,
                                        initial_validation_status = "validation-in-progress",
                                        error = string.Join(',', row.ErrorMessages ?? new List<string>()),                                      
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
       
        private UploadSummaryDto FromBatch(Batch<LASGPaymentRow> batch){
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
                ProductCode = batch.ProductCode,
                FileName = batch.NameOfFile
            };

        }
    }
}

//script changes
// TABLE: tbl_lirs_multi_tax_transactions_detail
// SPs:
// sp_insert_invalid_lirs_multitax
// sp_update_lasg_multitax_detail_enterprise_error
// sp_update_lasg_multitax_payments_detail