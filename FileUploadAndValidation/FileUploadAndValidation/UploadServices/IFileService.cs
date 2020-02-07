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
        UploadResult ValidateContent(IEnumerable<Row> contentRows, UploadResult uploadResult);

        //Task SaveRowsToDB(string scheduleId, IEnumerable<Row> contents);

        //Task UploadToNas(string scheduleId, IEnumerable<Row> contents, string contentType);

        Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows, UploadResult uploadResult);
    }
   
}