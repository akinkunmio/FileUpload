using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadApi.Services
{
    public interface IFileService
    {
        Task SaveToDBForReporting(Guid scheduleId, byte[] contents);

        Task SendToEventQueue(Guid scheduleId, byte[] contents);

        Task UploadToNas(Guid scheduleId, byte[] contents, string contentType);

        Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows, UploadResult uploadResult);
    }
   
}