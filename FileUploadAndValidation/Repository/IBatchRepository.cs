using FileUploadAndValidation.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Repository
{
    public interface IBatchRepository
    {
        void Save(string batchId, IList<RowDetail> validRows, IList<Failure> failures);
    }
}
