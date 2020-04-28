using Dapper;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Utils;
using FileUploadApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Repository
{
    public class FirsDbRepository : IFirsDbRepository
    {
        private readonly IAppConfig _appConfig;
        private readonly ILogger<FirsDbRepository> _logger;

        public FirsDbRepository(IAppConfig appConfig, ILogger<FirsDbRepository> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
        }

        public Task<BatchFileSummary> GetBatchUploadSummary(string batchId)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetBatchUploadSummaryId(string batchId)
        {
            throw new NotImplementedException();
        }

        public Task<BillPaymentRowStatusDtoObject> GetBillPaymentRowStatuses(string batchId, PaginationFilter pagination)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ConfirmedBillPaymentDto>> GetConfirmedBillPayments(string batchId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedData<BatchFileSummary>> GetUploadSummariesByUserId(string userId, PaginationFilter paginationFilter)
        {
            throw new NotImplementedException();
        }

        public async Task<string> InsertAllUploadRecords(UploadSummaryDto fileDetail, List<Firs> firsList, List<FailedFirs> failedFirsList, string validationType)
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

                            if (validationType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                            {
                                var firsWhtList = (IList<FirsWht>)firsList;

                                foreach (var firsWht in firsWhtList)
                                {
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

                                var failedFirsWhtList = (IList<FailedFirsWht>)failedFirsList;

                                foreach (var failedFirsWht in failedFirsWhtList)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_invalid_firs_wht",
                                        param: new
                                        {
                                            beneficiary_address = failedFirsWht.BeneficiaryAddress,
                                            beneficiary_name = failedFirsWht.BeneficiaryName,
                                            beneficiary_tin = failedFirsWht.BeneficiaryTin,
                                            contract_amount = failedFirsWht.ContractAmount,
                                            contract_date = failedFirsWht.ContractDate,
                                            contract_type = failedFirsWht.ContractType,
                                            wht_rate = failedFirsWht.WhtRate,
                                            wht_amount = failedFirsWht.WhtAmount,
                                            period_covered = failedFirsWht.PeriodCovered,
                                            invoice_number = failedFirsWht.InvoiceNumber,
                                            row_status = "Invalid",
                                            created_date = failedFirsWht.CreatedDate,
                                            row_num = failedFirsWht.RowNumber,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Invalid",
                                            error = failedFirsWht.Error
                                        },
                                        transaction: sqlTransaction,
                                        commandType: System.Data.CommandType.StoredProcedure);
                                }
                            }
                            else if (validationType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                            {
                                var firsWVatList = (IList<FirsWVat>)firsList;

                                foreach (var firsWVat in firsWVatList)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_valid_firs_wvat",
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

                                var failedFirsWhtList = (IList<FailedFirsWht>)failedFirsList;

                                foreach (var failedFirsWht in failedFirsWhtList)
                                {
                                    await connection.ExecuteAsync(sql: "sp_insert_invalid_firs_wht",
                                        param: new
                                        {
                                            beneficiary_address = failedFirsWht.BeneficiaryAddress,
                                            beneficiary_name = failedFirsWht.BeneficiaryName,
                                            beneficiary_tin = failedFirsWht.BeneficiaryTin,
                                            contract_amount = failedFirsWht.ContractAmount,
                                            contract_date = failedFirsWht.ContractDate,
                                            contract_type = failedFirsWht.ContractType,
                                            wht_rate = failedFirsWht.WhtRate,
                                            wht_amount = failedFirsWht.WhtAmount,
                                            period_covered = failedFirsWht.PeriodCovered,
                                            invoice_number = failedFirsWht.InvoiceNumber,
                                            row_status = "Invalid",
                                            created_date = failedFirsWht.CreatedDate,
                                            row_num = failedFirsWht.RowNumber,
                                            transactions_summary_Id = transactionSummaryId,
                                            initial_validation_status = "Invalid",
                                            error = failedFirsWht.Error
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

        public Task UpdateBillPaymentInitiation(string batchId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUploadSuccess(string batchId, string userValidationFileName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateValidationResponse(UpdateValidationResponseModel updateBillPayments)
        {
            throw new NotImplementedException();
        }
    }
}
