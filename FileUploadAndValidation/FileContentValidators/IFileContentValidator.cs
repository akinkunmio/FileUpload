using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.FileServices
{

    public interface IFileContentValidator
    {
        Task<UploadResult> Validate(FileUploadRequest uploadRequest, IEnumerable<Row> rows, UploadResult uploadResult);
    }
}
