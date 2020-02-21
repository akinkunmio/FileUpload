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
        private readonly IBillPaymentDbRepository _dbRepository;
        private readonly INasRepository _nasRepository;

        public ApiUploadService(Func<FileReaderTypeEnum, IFileReader> fileReader,
            Func<FileServiceTypeEnum, IFileService> fileService, 
            IBillPaymentDbRepository dbRepository,
            INasRepository nasRepository)
        {
            _dbRepository = dbRepository;
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
            BatchFileSummaryDto batchFileSummaryDto;

            try
            {
                batchFileSummaryDto = await _bulkBillPaymentService.GetBatchUploadSummary(batchId);
            }
            catch (Exception)
            {
                throw;
            }
            return batchFileSummaryDto;
        }

        public async Task<IEnumerable<BillPaymentRowStatus>> GetBillPaymentsStatus(string batchId, PaginationFilter pagination)
        {
            IEnumerable<BillPaymentRowStatus> billPaymentStatuses;

            try
            {
                billPaymentStatuses = await _bulkBillPaymentService.GetBillPaymentResults(batchId,pagination);
            }
            catch (Exception)
            {
                throw;
            }
            return billPaymentStatuses;
        }

        public async Task<UploadResult> UploadFileAsync(UploadOptions uploadOptions, Stream stream)
        {
            IEnumerable<Row> rows = new List<Row>();
            var uploadResult = new UploadResult();

            if(uploadOptions == null)
                throw new AppException("Upload options must be set!.");

            var batchId = GenericHelpers.GenerateBatchId(uploadOptions.FileName, DateTime.Now);

            uploadResult.BatchId = batchId;

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
                    return await _firsWhtService.Upload(uploadOptions, rows, uploadResult);
                case "autopay":
                    return await _autoPayService.Upload(uploadOptions, rows, uploadResult);
                case "sms":
                    return await _bulkSmsService.Upload(uploadOptions, rows, uploadResult);
                case "billpayment":
                    return await _bulkBillPaymentService.Upload(uploadOptions, rows, uploadResult);
                default:
                    throw new AppException("Content type not supported!.");
            }
            
        }

        public async Task PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions)
        {
            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));
            try
            {
                await _bulkBillPaymentService.PaymentInitiationConfirmed(batchId, initiatePaymentOptions);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

}
