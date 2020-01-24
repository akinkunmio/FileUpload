using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Services;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public class ApiUploadService : IApiUploadService
    {
        private readonly IFileService _firsWhtService;
        private readonly IFileService _autoPayService;
        private readonly IFileService _bulkBillPaymentService;
        private readonly IFileService _bulkSmsService;
        private readonly IFileReader _txtFileReader;
        private readonly IFileReader _csvFileReader;
        private readonly IFileReader _xlsxFileReader;
        private readonly IFileReader _xlsFileReader;

        public ApiUploadService(Func<FileReaderTypeEnum, IFileReader> fileReader,
            Func<FileServiceTypeEnum, IFileService> fileService)
        {
            _firsWhtService = fileService(FileServiceTypeEnum.FirsWht);
            _autoPayService = fileService(FileServiceTypeEnum.AutoPay);
            _bulkSmsService = fileService(FileServiceTypeEnum.BulkSMS);
            _bulkBillPaymentService = fileService(FileServiceTypeEnum.BulkBillPayment);
            _txtFileReader = fileReader(FileReaderTypeEnum.TXT);
            _csvFileReader = fileReader(FileReaderTypeEnum.CSV);
            _xlsxFileReader = fileReader(FileReaderTypeEnum.XLSX);
            _xlsFileReader = fileReader(FileReaderTypeEnum.XLS);
        }

        public async Task<UploadResult> UploadFileAsync(UploadOptions uploadOptions, FileTypes fileExtension, byte[] content)
        {
            ArgumentGuard.NotNull(fileExtension, nameof(fileExtension));
            IEnumerable<Row> rows;
            var uploadResult = new UploadResult();
            uploadOptions.ContentType = "FIRS_WHT";
            uploadOptions.ValidateHeaders = true;

            switch (fileExtension)
            {
                case FileTypes.TXT:
                    rows = _txtFileReader.Read(content);
                    break;
                case FileTypes.CSV:
                    rows = _csvFileReader.Read(content);
                    break;
                case FileTypes.XLSX:
                    rows = _xlsxFileReader.Read(content);
                    break;
                case FileTypes.XLS:
                    rows = _xlsFileReader.Read(content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("File type not supported");
            }

            switch (uploadOptions.ContentType.ToUpperInvariant())
            {
                case "FIRS_WHT":
                    return await _firsWhtService.Upload(uploadOptions, rows);
                case "AUTOPAY":
                    return await _autoPayService.Upload(uploadOptions, rows);
                case "BULKSMS":
                    return await _bulkSmsService.Upload(uploadOptions, rows);
                case "BULKBILLPAYMENT":
                    return await _bulkBillPaymentService.Upload(uploadOptions, rows);
                default:
                    throw new ArgumentOutOfRangeException("Content type not supported!.");
            }
        }
    }

    public enum FileTypes { XLS, XLSX, TXT, CSV }
   
}
