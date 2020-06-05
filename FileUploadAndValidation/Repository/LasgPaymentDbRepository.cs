using System.Threading.Tasks;
using FileUploadAndValidation.BillPayments;
using FileUploadAndValidation.Models;

namespace FileUploadAndValidation.Repository
{
    public class LasgPaymentDbRepository : IDetailsDbRepository<LASGPaymentRow>
    {
        public async Task<string> InsertAllUploadRecords(Batch<LASGPaymentRow> batch)
        {
            await Task.CompletedTask;
            return "";
        }
    }
}