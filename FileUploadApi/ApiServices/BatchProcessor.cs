using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.FileReaders;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using FileUploadApi.Models;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Repository;
using FileUploadApi.Controllers;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace FileUploadApi.ApiServices
{
    public class BatchProcessor : IBatchProcessor
    {
        private readonly IEnumerable<IFileContentValidator<BillPayment>> _fileContentValidators;
        private readonly IBatchRepository _batchRepository;

        public BatchProcessor(IBatchRepository batchRepository, IEnumerable<IFileContentValidator<BillPayment>> fileContentValidators)
        {
            _batchRepository = batchRepository;
            _fileContentValidators = fileContentValidators;
        }

        public async Task<ValidationResult<BillPayment>> UploadFileAsync(IEnumerable<Row> rows, IFileUploadRequest request)
        {
            #region commented for now
            //ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));

            //if (!ContentSupported(request.ContentType))
            //    throw new AppException("Invalid Content Type specified");

            ////use either validationtype or contentype name
            //if (request.ContentType.ToLower().Equals(GenericConstants.WHT.ToLower())
            //    || request.ContentType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
            //    request.ContentType = GenericConstants.Firs;
            #endregion

            var batchId = GenericHelpers.GenerateBatchId("QTB", DateTime.Now);
            var fileContentValidator = _fileContentValidators.First();//.FirstOrDefault(r => r.CanProcess(request.ContentType)) ?? throw new AppException("Invalid file content");
            var uploadResult = await fileContentValidator.Validate(rows);//, request);
            //await _batchRepository.Save(batchId, request, uploadResult.ValidRows, uploadResult.Failures);

            return uploadResult;
        }

        private bool ContentSupported(string contentType)
        {
            if (string.IsNullOrEmpty(contentType)) return false;

            var supported = new[] {
                GenericConstants.BillPaymentIdPlusItem.ToLower(),
                GenericConstants.BillPaymentId.ToLower(),
                GenericConstants.WVAT.ToLower(),
                GenericConstants.WHT.ToLower()
            };

            return supported.Any(c => c == contentType.ToLower());
        }


        private void Test()
        {
            // FILE CONTENT TYPES
            //autopay
            //firs
            //enterprise
            //bulk-sms

            // var rows = ReadFromFile();
            // var validationResults = val.Validate(rows);//Rows and error message
            // db.Save(validationResults); ::: firs, autopay, etc
        }

        Task<UploadResult> IBatchProcessor.UploadFileAsync(IEnumerable<Row> rows, IFileUploadRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
