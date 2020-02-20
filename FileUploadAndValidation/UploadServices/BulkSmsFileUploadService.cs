using FileUploadAndValidation.Models;
using FileUploadApi.Services;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadApi
{
    public class BulkSmsFileService : IFileService
    {
        public Task SaveRowsToDB(Guid scheduleId, byte[] contents)
        {
            throw new NotImplementedException();
        }

        public Task SaveRowsToDB(string scheduleId, IEnumerable<Row> contents)
        {
            throw new NotImplementedException();
        }

        public Task SendToEventQueue(Guid scheduleId, byte[] contents)
        {
            throw new NotImplementedException();
        }

        public Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            throw new System.NotImplementedException();
        }

      
        public async Task<UploadResult> ValidateContent(IEnumerable<Row> contentRows, UploadResult uploadResult)
        {
            throw new NotImplementedException();
        }

        public Task<ValidateRowsResult> ValidateContent(IEnumerable<Row> contentRows)
        {
            throw new NotImplementedException();
        }
    }
}