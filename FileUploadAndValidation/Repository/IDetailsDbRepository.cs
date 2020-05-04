using System.Collections.Generic;
using System.Threading.Tasks;
using FileUploadApi;

namespace FileUploadAndValidation.Repository
{
    public interface IDetailsDbRepository<T>
    {
        Task<string> InsertAllUploadRecords(UploadSummaryDto fileDetail, 
                                            IList<T> validRows, 
                                            IList<T> invalidRows,
                                            string itemType = null);
    }
}