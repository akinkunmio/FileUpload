using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.FileReaderImpl.CsvTxtMappers;
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

namespace FileUploadApi.ApiServices
{
    public class ApiUploadService : IApiUploadService
    {
        private readonly IFileReader _txtCsvFileReader;
        private readonly IFileReader _xlsxFileReader;
        private readonly IFileReader _xlsFileReader;
        private readonly IFileService _firsWhtService;
        private readonly IFileService _autoPayService;
        private readonly IFileService _bulkBillPaymentService;
        private readonly IFileService _bulkSmsService;
        private readonly IBillPaymentDbRepository _dbRepository;

        public ApiUploadService(Func<FileReaderTypeEnum, IFileReader> fileReader,
            Func<FileServiceTypeEnum, IFileService> fileService, 
            IBillPaymentDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
            _txtCsvFileReader = fileReader(FileReaderTypeEnum.TXT_CSV);
            _xlsxFileReader = fileReader(FileReaderTypeEnum.XLSX);
            _xlsFileReader = fileReader(FileReaderTypeEnum.XLS);
            _firsWhtService = fileService(FileServiceTypeEnum.FirsWht);
            _autoPayService = fileService(FileServiceTypeEnum.AutoPay);
            _bulkSmsService = fileService(FileServiceTypeEnum.BulkSMS);
            _bulkBillPaymentService = fileService(FileServiceTypeEnum.BulkBillPayment);
        }

        public async Task<BatchFileSummaryDto> GetBatchFileSummary(string scheduleId, string userName)
        {
            BatchFileSummary batchFileSummary;
            var batchFileSummaryDto = new BatchFileSummaryDto();
            try
            {
                batchFileSummary = await _dbRepository.GetBatchUploadSummary(scheduleId, userName);
                batchFileSummaryDto = new BatchFileSummaryDto
                {
                    BatchId = batchFileSummary.BatchId,
                    ContentType = batchFileSummary.ContentType,
                    ItemType = batchFileSummary.ItemType,
                    NumOfAllRecords = batchFileSummary.NumOfAllRecords,
                    NumOfValidRecords = batchFileSummary.NumOfValidRecords,
                    Status = batchFileSummary.Status,
                    UploadDate = batchFileSummary.UploadDate
                };
            }
            catch (Exception)
            {

            }
            return batchFileSummaryDto;
        }

        public async Task<IEnumerable<BillPaymentStatus>> GetBillPaymentsStatus(string batchId, string userName)
        {
            IEnumerable<BillPayment> billPayments = new List<BillPayment>();
            IEnumerable<BillPaymentStatus> billPaymentStatuses = default;

            try
            {
                billPayments = await _dbRepository
                    .GetBillPayments(batchId, userName);

                billPaymentStatuses = billPayments
                    .Select(p => new BillPaymentStatus 
                    {
                         ErrorResponse = p.EnterpriseErrorResponse,
                         RowNumber = p.RowNumber,
                         ReferenceId = p.EnterpriseReferenceId,
                         Status = p.Status,
                         UploadDate = p.CreatedDate
                    });
            }
            catch(Exception)
            {

            }
            return billPaymentStatuses;
        }

        public async Task<UploadResult> UploadFileAsync(UploadOptions uploadOptions, Stream stream)
        {
            IEnumerable<Row> rows = new List<Row>();
            var uploadResult = new UploadResult();

            if(uploadOptions == null)
                throw new AppException("Upload options must be set!.");

            uploadOptions.ContentType = "BILLPAYMENT";
            uploadOptions.ValidateHeaders = true;

            var fileExtension = Path.GetExtension(uploadOptions.FileName).Replace(".", string.Empty).ToLower();

            switch (fileExtension)
            {
                case "txt":
                case "csv":
                    rows = _txtCsvFileReader.Read(stream);
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
                    return await _firsWhtService.Upload(uploadOptions: uploadOptions, rows: rows, uploadResult: uploadResult);
                case "autopay":
                    return await _autoPayService.Upload(uploadOptions: uploadOptions, rows: rows, uploadResult: uploadResult);
                case "sms":
                    return await _bulkSmsService.Upload(uploadOptions: uploadOptions, rows: rows, uploadResult: uploadResult);
                case "billpayment":
                    return await _bulkBillPaymentService.Upload(uploadOptions: uploadOptions, rows: rows, uploadResult: uploadResult);
                default:
                    throw new AppException("Content type not supported!.");
            }
        }

    }

}
