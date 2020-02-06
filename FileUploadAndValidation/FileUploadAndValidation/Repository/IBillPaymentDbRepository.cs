using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace FileUploadApi
{
    public class BillPaymentRepository : IBillPaymentDbRepository
    {
        public BillPaymentRepository()
        {
                
        }
        public Task<string> AddBatchFileInfo(BatchFileSummary fileDetail)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> AddBillPaymentRecords(BillPayment billPayments)
        {
            throw new System.NotImplementedException();
        }

        public Task<BatchFileSummary> GetBatchFileSummary(string batchId, string userName)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<BillPayment>> GetBillPayments(string batchId, string userName)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateBatchFileValidationResult(string batchId, ValidationResponse validationResponse)
        {
            throw new System.NotImplementedException();
        }

        //public Task UpdateBatchRecords(string batchId, ValidationResponse validationResponse)
        //{
        //    //if the validationResponse result is null 
        //    //the stattus is pending
        //    //else add summary to db
        //    throw new System.NotImplementedException();
        //}
    }
    public interface IBillPaymentDbRepository
    {
        Task<string> AddBatchFileInfo(BatchFileSummary fileDetail);

        Task<string> AddBillPaymentRecords(BillPayment billPayments);

        Task UpdateBatchFileValidationResult(string batchId, ValidationResponse validationResponse);

        Task<List<BillPayment>> GetBillPayments(string batchId, string userName);

        Task<BatchFileSummary> GetBatchFileSummary(string batchId, string userName);
    }
   
   
}