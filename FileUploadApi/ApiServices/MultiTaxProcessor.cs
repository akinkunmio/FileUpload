using FileUploadAndValidation.FileServices;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
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
        private readonly IBatchRepository _batchRepository;
        private readonly IEnumerable<IFileReader> _fileReaders;
        private readonly IEnumerable<IFileContentValidator> _fileContentValidators;


        public MultiTaxProcessor(IEnumerable<IFileReader> fileReaders,
            IEnumerable<IFileContentValidator> fileContentValidators,
            IEnumerable<IBatchRepository> batchRepositories
            )
        {
            _fileReaders = fileReaders;
            _fileContentValidators = fileContentValidators;
            _batchRepository = batchRepositories.ToArray()[1];
        }

        public async Task<ResponseResult> UploadFileAsync(FileUploadRequest request)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(request.ItemType, nameof(request.ItemType));
            ArgumentGuard.NotNullOrWhiteSpace(request.AuthToken, nameof(request.AuthToken));

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
                IEnumerable<Row> rows = ExtractFileContent(request.FileExtension, contentStream);

                await ValidateFileContentAsync(request, rows, uploadResult);

                await _batchRepository.Save(uploadResult, request);

                return ResponseResult.CreateResponseResult(uploadResult, request.ContentType, request.ItemType);
            }

        }

        private async Task ValidateFileContentAsync(FileUploadRequest request, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            switch (request.ContentType.ToLower())
            {
                case GenericConstants.Firs:
                    await _fileContentValidators.ToArray()[2].Validate(request, rows, uploadResult);
                    break;
                case GenericConstants.Lirs:
                    await _fileContentValidators.ToArray()[3].Validate(request, rows, uploadResult);
                    break;
                default:
                    throw new AppException("Authority type not supported!.");
            }
        }


        private IEnumerable<Row> ExtractFileContent(string fileExtension, Stream contentStream)
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
