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

namespace FileUploadApi.ApiServices
{
    public class ApiUploadService : IApiUploadService
    {
        private readonly IFileReader _txtFileReader;
        private readonly IFileReader _csvFileReader;
        private readonly IFileReader _xlsxFileReader;
        private readonly IFileReader _xlsFileReader;
        private readonly IFileService _firsWhtService;
        private readonly IFileService _autoPayService;
        private readonly IFileService _bulkBillPaymentService;
        private readonly IFileService _bulkSmsService;
        private readonly INasRepository _nasRepository;

        public ApiUploadService(Func<FileReaderTypeEnum, IFileReader> fileReader,
            Func<FileServiceTypeEnum, IFileService> fileService, 
            INasRepository nasRepository)
        {
            _txtFileReader = fileReader(FileReaderTypeEnum.TXT);
            _csvFileReader = fileReader(FileReaderTypeEnum.CSV);
            _xlsxFileReader = fileReader(FileReaderTypeEnum.XLSX);
            _xlsFileReader = fileReader(FileReaderTypeEnum.XLS);
            _firsWhtService = fileService(FileServiceTypeEnum.FirsWht);
            _autoPayService = fileService(FileServiceTypeEnum.AutoPay);
            _bulkSmsService = fileService(FileServiceTypeEnum.BulkSMS);
            _bulkBillPaymentService = fileService(FileServiceTypeEnum.BulkBillPayment);
            _nasRepository = nasRepository;
        }

        public async Task<BatchFileSummaryDto> GetFileSummary(string batchId)
        {
            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));

            BatchFileSummaryDto batchFileSummaryDto;

            batchFileSummaryDto = await _bulkBillPaymentService.GetBatchUploadSummary(batchId);
           
            return batchFileSummaryDto;
        }

        public async Task<IEnumerable<BillPaymentRowStatus>> GetBillPaymentsStatus(string batchId, PaginationFilter pagination)
        {
            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));
            ArgumentGuard.NotDefault(pagination.PageNumber, nameof(pagination.PageNumber));
            ArgumentGuard.NotDefault(pagination.PageSize, nameof(pagination.PageSize));

            IEnumerable<BillPaymentRowStatus> billPaymentStatuses;

            try
            {
                billPaymentStatuses = await _bulkBillPaymentService.GetBillPaymentResults(batchId, pagination);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            return billPaymentStatuses;
        }

        public async Task<UploadResult> UploadFileAsync(UploadOptions uploadOptions, Stream stream)
        {
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.AuthToken, nameof(uploadOptions.AuthToken));
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ContentType, nameof(uploadOptions.ContentType));
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.FileExtension, nameof(uploadOptions.FileExtension));
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.FileName, nameof(uploadOptions.FileName));
            ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ItemType, nameof(uploadOptions.ItemType));
            ArgumentGuard.NotDefault(stream.Length, nameof(stream.Length));

            if (!uploadOptions.ItemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower())
                && !uploadOptions.ItemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower()))
                throw new AppException("Invalid Item Type specified");

            IEnumerable<Row> rows = new List<Row>();
            var uploadResult = new UploadResult();

            var batchId = GenericHelpers.GenerateBatchId(uploadOptions.FileName, DateTime.Now);

            uploadOptions.RawFileLocation = await _nasRepository.SaveRawFile(batchId, stream, uploadOptions.FileExtension);
            stream.Seek(0, SeekOrigin.Begin);

            switch (uploadOptions.FileExtension)
            {
                case "txt":
                    rows = _txtFileReader.Read(stream);
                    break;
                case "csv":
                    rows = _csvFileReader.Read(stream);
                    break;
                case "xlsx":
                    rows = _xlsxFileReader.Read(stream);
                    break;
                case "xls":
                    rows = _xlsFileReader.Read(stream);
                    break;
                default:
                    throw new AppException("File extension not supported!.");
            }

            switch (uploadOptions.ContentType.ToLower())
            {
                case "firs_wht":
                    return await _firsWhtService.Upload(uploadOptions, rows, batchId);
                case "autopay":
                    return await _autoPayService.Upload(uploadOptions, rows, batchId);
                case "sms":
                    return await _bulkSmsService.Upload(uploadOptions, rows, batchId);
                case "billpayment":
                    uploadResult = await _bulkBillPaymentService.Upload(uploadOptions, rows, batchId);
                    break;
                default:
                    throw new AppException("Content type not supported!.");
            }

            return uploadResult;
        }

        public async Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions)
        {
            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));
            ArgumentGuard.NotNullOrWhiteSpace(initiatePaymentOptions.AuthToken, nameof(initiatePaymentOptions.AuthToken));
            ArgumentGuard.NotNull(initiatePaymentOptions.BusinessId, nameof(initiatePaymentOptions.BusinessId));
            ArgumentGuard.NotNull(initiatePaymentOptions.ApprovalConfigId, nameof(initiatePaymentOptions.ApprovalConfigId));
            ArgumentGuard.NotNull(initiatePaymentOptions.UserId, nameof(initiatePaymentOptions.UserId));
            ArgumentGuard.NotNullOrWhiteSpace(initiatePaymentOptions.UserName, nameof(initiatePaymentOptions.UserName));

             return await _bulkBillPaymentService.PaymentInitiationConfirmed(batchId, initiatePaymentOptions);
        }
    }

}
