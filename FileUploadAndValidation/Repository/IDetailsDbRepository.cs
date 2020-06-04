using System.Collections.Generic;
using System.Threading.Tasks;
using FileUploadAndValidation.Models;
using FileUploadApi;

namespace FileUploadAndValidation.Repository
{
    public interface IDetailsDbRepository<T> where T : ValidatedRow
    {
        Task<string> InsertAllUploadRecords(Batch<T> batch);
    }
}