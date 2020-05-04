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
        public async Task<string> InsertAllUploadRecords(UploadSummaryDto fileDetail, IList<AutoPayRow> validRows, IList<AutoPayRow> invalidRows, string itemType = null)
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
                            var transactionSummaryId = await _batchRepository.CreateBatchSummary((connection, sqlTransaction), fileDetail);

                            foreach (var row in validRows)
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
                                        transactions_summary_Id = transactionSummaryId,
                                        initial_validation_status = "Invalid",
                                        error =  "0",//row.Error
                                    },
                                    transaction: sqlTransaction,
                                    commandType: System.Data.CommandType.StoredProcedure);
                            }

                            foreach (var row in invalidRows)
                            {
                                await connection.ExecuteAsync(sql: "sp_insert_invalid_bill_payments",
                                    param: new
                                    {
                                        product_code = "0",//row.ProductCode,
                                        item_code =  "0",//.ItemCode,
                                        customer_id =  "0",//row.CustomerId,
                                        amount = row.Amount,
                                        row_status = "Invalid",
                                        created_date =  DateTime.Now,//row.CreatedDate,
                                        row_num =  "0",//row.RowNumber,
                                        transactions_summary_Id = transactionSummaryId,
                                        initial_validation_status = "Invalid",
                                        error =  "0",//row.Error
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
    }
}