using FileUploadAndValidation;
using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadApi.Processors
{
    public interface IBatchFileProcessor<TContext>
    {
        Task<BatchFileSummary> UploadAsync(IEnumerable<Row> rows, TContext context, string clientToken = "");
    }
}
