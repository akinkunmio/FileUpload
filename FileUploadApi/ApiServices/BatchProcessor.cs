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
        private readonly IFileContentValidator _fileContentValidator;
        private readonly INasRepository _nasRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IFileReader _fileReader;


        public BatchProcessor(IBatchRepository batchRepository, INasRepository nasRepository, IFileContentValidator fileContentValidator)
        {
            _nasRepository = nasRepository;
            _batchRepository = batchRepository;
            _fileContentValidator = fileContentValidator;
        }

        public async Task<UploadResult> UploadFileAsync(HttpRequest httpRequest)
        {
            var request = FileUploadRequest.FromRequest(httpRequest);

            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));

            if (!request.ContentType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower())
                && !request.ContentType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                && !request.ContentType.ToLower().Equals(GenericConstants.WVAT.ToLower())
                && !request.ContentType.ToLower().Equals(GenericConstants.WHT.ToLower()))
                throw new AppException("Invalid Content Type specified");

            if (request.UserId == null)
                throw new AppException("Id cannot be null");

            //use either validationtype or contentype name
            if (request.ContentType.ToLower().Equals(GenericConstants.WHT.ToLower())
                || request.ContentType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                request.ContentType = GenericConstants.Firs;

            IEnumerable<Row> rows = default;
            var uploadResult = new UploadResult();

            var batchId = GenericHelpers.GenerateBatchId(request.FileName, DateTime.Now);

            using(var contentStream = request.FileRef.OpenReadStream())
            {
                var rawFileLocation = _nasRepository.SaveRawFile(batchId, contentStream, request.FileExtension);
                rows = _fileReader.Read(contentStream);
                uploadResult = await _fileContentValidator.Validate(request,rows);

                await _batchRepository.Save(batchId, uploadResult.ValidRows, uploadResult.Failures);

                return uploadResult;
            }
        }

        //private void Switch()
        //{
        //    switch (uploadOptions.FileExtension)
        //    {
        //        case "txt":
        //            rows = _txtFileReader.Read(stream);
        //            break;
        //        case "csv":
        //            rows = _csvFileReader.Read(stream);
        //            break;
        //        case "xlsx":
        //            rows = _xlsxFileReader.Read(stream);
        //            break;
        //        case "xls":
        //            rows = _xlsFileReader.Read(stream);
        //            break;
        //        default:
        //            throw new AppException("File extension not supported!.");
        //    }

        //    switch (uploadOptions.ContentType.ToLower())
        //    {
        //        case "firs":
        //            uploadResult = await _firsService.Upload(uploadOptions, rows, uploadResult);
        //            break;
        //        case "autopay":
        //            return await _autoPayService.Upload(uploadOptions, rows, uploadResult);
        //        case "sms":
        //            return await _bulkSmsService.Upload(uploadOptions, rows, uploadResult);
        //        case "billpayment":
        //            uploadResult = await _bulkBillPaymentService.Upload(uploadOptions, rows, uploadResult);
        //            break;
        //        default:
        //            throw new AppException("Content type not supported!.");
        //    }
        //}

    }

}
