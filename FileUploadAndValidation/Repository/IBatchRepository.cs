using FileUploadAndValidation.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Repository
{
    public interface IBatchRepository
    {
        Task Save(UploadResult uploadResult, FileUploadRequest request);
    }
}
