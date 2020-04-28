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
        private readonly IBatchRepository _batchRepository;
        private readonly IEnumerable<IFileReader> _fileReader;

        public BatchProcessor(IBatchRepository batchRepository, IFileContentValidator fileContentValidator,
            IEnumerable<IFileReader> fileReader)
        {
            _batchRepository = batchRepository;
            _fileContentValidator = fileContentValidator;
            _fileReader = fileReader;
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

              //use either validationtype or contentype name
            if (request.ContentType.ToLower().Equals(GenericConstants.WHT.ToLower())
                || request.ContentType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
                request.ContentType = GenericConstants.Firs;

            IEnumerable<Row> rows = default;
            var uploadResult = new UploadResult();

            var batchId = GenericHelpers.GenerateBatchId(request.FileName, DateTime.Now);

            using(var contentStream = request.FileRef.OpenReadStream())
            {
                rows = GetRows(request.FileExtension, contentStream);

                uploadResult = await _fileContentValidator.Validate(request,rows);

                await _batchRepository.Save(batchId, request, uploadResult.ValidRows, uploadResult.Failures);

                return uploadResult;
            }
        }

        private IEnumerable<Row> GetRows(string fileExtension, Stream contentStream)
        {
            switch (fileExtension)
            {
                case "txt":
                    return _fileReader.ToArray()[0].Read(contentStream);
                case "csv":
                    return _fileReader.ToArray()[1].Read(contentStream);
                case "xlsx":
                    return _fileReader.ToArray()[2].Read(contentStream);
                case "xls":
                    return _fileReader.ToArray()[3].Read(contentStream);
                default:
                    throw new AppException("File extension not supported!.");
            }
        }

    }
}
