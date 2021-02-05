using FileUploadAndValidation.FileServices;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.Utils;
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
    public interface ISingleTaxProcessor
    {
        Task<ResponseResult> UploadFileAsync(FileUploadRequest uploadRequest);
    }
    public class SingleTaxProcessor : ISingleTaxProcessor
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IEnumerable<IFileReader> _fileReaders;
        private readonly IEnumerable<IFileContentValidator> _fileContentValidators;
        private readonly ApprovalUtil _approvalUtil;

        public SingleTaxProcessor(IEnumerable<IFileReader> fileReaders,
            IEnumerable<IFileContentValidator> fileContentValidators,
            IEnumerable<IBatchRepository> batchRepositories, ApprovalUtil approvalUtil)
        {
            _fileReaders = fileReaders;
            _fileContentValidators = fileContentValidators;
            _batchRepository = batchRepositories.ToArray()[2];
            _approvalUtil = approvalUtil;
        }

        public async Task<ResponseResult> UploadFileAsync(FileUploadRequest request)
        {
            ArgumentGuard.NotNullOrWhiteSpace(request.ContentType, nameof(request.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(request.ItemType, nameof(request.ItemType));
            ArgumentGuard.NotNullOrWhiteSpace(request.AuthToken, nameof(request.AuthToken));
            ArgumentGuard.NotNullOrWhiteSpace(request.AdditionalData, nameof(request.AdditionalData));

            if (!request.ContentType.ToLower().Equals(GenericConstants.Firs)
               && !request.ContentType.ToLower().Equals(GenericConstants.Lasg)
               && !request.ContentType.ToLower().Equals(GenericConstants.FctIrs))
                throw new AppException("Invalid Authority Type specified.");

            var uploadResult = new UploadResult
            {
                FileName = request.FileName
            };

            using (var contentStream = request.FileRef.OpenReadStream())
            {
                IEnumerable<Row> rows = ExtractFileContent(request.FileExtension, contentStream);

                await ValidateFileContentAsync(request, rows, uploadResult);

                var totalAmount = (long)uploadResult.ValidRows.Select(p => GenericHelpers.GetAmountFromSingleTaxRow(p)).ToList().Sum(s => s);

                var approvalResponse = await _approvalUtil.GetApprovalConfiguration(request.BusinessId, request.UserId, totalAmount);

                if (!approvalResponse)
                    throw new AppException("User is not enabled to upload a file", 400);

                var itemType = uploadResult.ValidRows.Count > 0 ? uploadResult.ValidRows.FirstOrDefault().TaxType : uploadResult.Failures.FirstOrDefault().Row.TaxType;

                if (!request.AdditionalData.ToLower().Equals(itemType.ToLower()))
                    throw new AppException("Tax selected does not match file content", 400);

                uploadResult.BatchId = GenericHelpers.GenerateBatchId($"QTB_FIRS_{itemType.ToUpper()}", DateTime.Now);
                await _batchRepository.Save(uploadResult, request);
                
                return ResponseResult.CreateResponseResult(uploadResult, request.ContentType, request.ItemType);
            }

        }

        private async Task ValidateFileContentAsync(FileUploadRequest request, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            switch (request.ContentType.ToLower())
            {
                case GenericConstants.Firs:
                    await _fileContentValidators.ToArray()[4].Validate(request, rows, uploadResult);
                    return;
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
