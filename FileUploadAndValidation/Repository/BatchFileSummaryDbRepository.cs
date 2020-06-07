using Dapper;
using static Dapper.DefaultTypeMap;
using System.Data.SqlClient;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Utils;
using Microsoft.Extensions.Logging;
using System.Data;
using FilleUploadCore.Exceptions;
using System;
using System.Threading.Tasks;
using FileUploadApi;
using System.Net;
using System.Linq;

namespace FileUploadAndValidation.Repository
{
    public class BatchFileSummaryDbRepository
    {
        private readonly IAppConfig _appConfig;
        private readonly ILogger<BatchFileSummaryDbRepository> _logger;

        public BatchFileSummaryDbRepository(IAppConfig appConfig, ILogger<BatchFileSummaryDbRepository> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
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

        public async Task<string> CreateBatchSummary((SqlConnection connection, SqlTransaction transaction) connectionDetails, UploadSummaryDto fileDetail)
        {
            var transactionSummaryId = await connectionDetails.connection.ExecuteScalarAsync(sql: "sp_insert_bill_payment_transaction_summary",
            param: new
            {
                batch_id = fileDetail.BatchId,
                status = fileDetail.Status,
                item_type = fileDetail.ItemType,
                num_of_records = fileDetail.NumOfAllRecords,
                upload_date = fileDetail.UploadDate,
                content_type = fileDetail.ContentType,
                nas_raw_file = fileDetail.NasRawFile,
                userid = fileDetail.UserId,
                product_code = fileDetail.ProductCode,
                product_name = fileDetail.ProductName,
                file_name = fileDetail.FileName, 
            },
            transaction: connectionDetails.transaction,
            commandType: System.Data.CommandType.StoredProcedure);

            return transactionSummaryId.ToString();  
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
    }
}