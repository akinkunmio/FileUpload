using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public class MultiTaxProcessor : IMultiTaxProcessor
    {
        private readonly IEnumerator<IFileReader> _fileReaders;

        public MultiTaxProcessor(IEnumerator<IFileReader> fileReaders)
        {
            _fileReaders = fileReaders;
        }

        public Task<ResponseResult> UploadFileAsync(FileUploadRequest request)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(request.ItemType, nameof(request.ItemType));
            ArgumentGuard.NotNullOrWhiteSpace(request.AuthToken, nameof(request.AuthToken));
            ArgumentGuard.NotNullOrWhiteSpace(request.BusinessTin, nameof(request.BusinessTin));


            if (!request.ContentType.ToLower().Equals(GenericConstants.Firs)
               && !request.ContentType.ToLower().Equals(GenericConstants.Lirs)
               && !request.ContentType.ToLower().Equals(GenericConstants.FCTirs))
                throw new AppException("Invalid Authority Type specified");

            var uploadResult = new UploadResult
            {
                FileName = request.FileName
            };

            uploadResult.BatchId = GenericHelpers.GenerateBatchId(request.FileName, DateTime.Now);

            using (var contentStream = request.FileRef.OpenReadStream())
            {
                IEnumerable<Row> rows = GetRows(request.FileExtension, contentStream);

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
                    RowsCount = uploadResult.RowsCount
                };
            }

        }

        private IEnumerable<Row> GetRows(string fileExtension, Stream contentStream)
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
    }
}
