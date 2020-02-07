using FileUploadAndValidation.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Repository
{
    public class NasRepository : INasRepository
    {
        public NasRepository()
        {

        }

        public Task<FileProperties> SaveAsJsonFile(string batchId, IEnumerable<BillPayment> billPayments)
        {
            throw new NotImplementedException();
        }
    }
    public interface INasRepository
    {
        Task<FileProperties> SaveAsJsonFile(string batchId, IEnumerable<BillPayment> billPayments);
    }

}
