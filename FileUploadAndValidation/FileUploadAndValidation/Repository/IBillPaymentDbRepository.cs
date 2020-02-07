using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace FileUploadApi
{
  
    public interface IBillPaymentDbRepository
    {
        Task<string> CreateBatchPaymentUpload(BatchFileSummary fileDetail, List<BillPayment> billPayments);

        Task<BatchFileSummary> GetBatchUploadSummary(string batchId, string userName);

        Task<IEnumerable<BillPayment>> GetBillPayments(long id);

        Task<IEnumerable<BillPayment>> GetBillPayments(string batchId, string userName);

        Task UpdateBatchUpload(UpdateBillPaymentsCollection updateBillPayments);
    }
   
   
}