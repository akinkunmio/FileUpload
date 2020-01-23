using FileUploadAndValidation.Models;
using FileUploadApi.Services;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadApi
{
    public class AutoPayFileService : IFileService
    {
        public Task SaveToDBForReporting(Guid scheduleId, byte[] contents)
        {
            throw new NotImplementedException();
        }

        public Task SendToEventQueue(Guid scheduleId, byte[] contents)
        {
            throw new NotImplementedException();
        }

        public Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows)
        {
            throw new System.NotImplementedException();
        }

        public Task UploadToNas(Guid scheduleId, byte[] contents, string contentType)
        {
            throw new NotImplementedException();
        }
    }
}