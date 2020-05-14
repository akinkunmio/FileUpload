using FileUploadAndValidation.Models;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.FileServices;

namespace FileUploadApi.ApiServices
{
    public class BatchProcessor : IBatchProcessor
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IEnumerable<IFileContentValidator> _fileContentValidators;
        private readonly IEnumerable<IFileReader> _fileReaders;

        public BatchProcessor(IBatchRepository batchRepository,
            IEnumerable<IFileContentValidator> fileContentValidators,
            IEnumerable<IFileReader> fileReaders)
        {
            _batchRepository = batchRepository;
            _fileContentValidators = fileContentValidators;
            _fileReaders = fileReaders;
        }

        public async Task<ResponseResult> UploadFileAsync(FileUploadRequest request)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(request.ItemType, nameof(request.ItemType));
            ArgumentGuard.NotNullOrWhiteSpace(request.AuthToken, nameof(request.AuthToken));

            if (!request.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower())
                && !request.ItemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
                && !request.ItemType.ToLower().Equals(GenericConstants.Wvat.ToLower())
                && !request.ItemType.ToLower().Equals(GenericConstants.Wht.ToLower()))
                throw new AppException("Invalid Content Type specified");

            if (request.ContentType.ToLower().Equals(GenericConstants.Firs.ToLower()))
                ArgumentGuard.NotNullOrWhiteSpace(request.BusinessTin, nameof(request.BusinessTin));
            
            var uploadResult = new UploadResult
            { 
                ProductCode = request.ProductCode, 
                ProductName = request.ProductName, 
                FileName = request.FileName 
            };

            uploadResult.BatchId = GenericHelpers.GenerateBatchId(request.FileName, DateTime.Now);

            using (var contentStream = request.FileRef.OpenReadStream())
            {
                IEnumerable<Row> rows = ExctractFileContent(request.FileExtension, contentStream);

                await ValidateFileContentAsync(request, rows, uploadResult);

                await _batchRepository.Save(uploadResult, request);

                return new ResponseResult
                {
                    BatchId = uploadResult.BatchId,
                    ValidRows = uploadResult.ValidRows
                                        .Select(row => GenericHelpers.RowMarshaller(row, request.ContentType, request.ItemType))
                                        .ToList(),
                    Failures = uploadResult.Failures.Select(a => new ResponseResult.FailedValidation
                    {
                        ColumnValidationErrors = a.ColumnValidationErrors,
                        Row = GenericHelpers.RowMarshaller(a.Row, request.ContentType, request.ItemType)
                    }).ToList(),
                    ErrorMessage = uploadResult.ErrorMessage,
                    FileName = uploadResult.FileName,
                    ProductCode = uploadResult.ProductCode,
                    ProductName = uploadResult.ProductName,
                    RowsCount = uploadResult.RowsCount
                };
            }
        }

        private IEnumerable<Row> ExctractFileContent(string fileExtension, Stream contentStream)
        {
            switch (fileExtension)
            {
                case "txt":
                    return _fileReaders.ToArray()[0].Read(contentStream);
                case "csv":
                    return _fileReaders.ToArray()[1].Read(contentStream);
                case "xlsx":
                    return _fileReaders.ToArray()[2].Read(contentStream);
                case "xls":
                    return _fileReaders.ToArray()[3].Read(contentStream);
                default:
                    throw new AppException("File extension not supported!.");
            }
        }

        private async Task<UploadResult> ValidateFileContentAsync(FileUploadRequest request, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            switch (request.ContentType.ToLower())
            {
                case "billpayment":
                    return await _fileContentValidators.ToArray()[0].Validate(request, rows, uploadResult);
                case "firs":
                    return await _fileContentValidators.ToArray()[1].Validate(request, rows, uploadResult);
                default:
                    throw new AppException("Content type not supported!.");
            }
        }
    }
}
