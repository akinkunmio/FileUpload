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

namespace FileUploadApi.ApiServices
{
    public class ApiUploadService //: IApiUploadService
    {
    //    private readonly IFileReader _txtFileReader;
    //    private readonly IFileReader _csvFileReader;
    //    private readonly IFileReader _xlsxFileReader;
    //    private readonly IFileReader _xlsFileReader;
    //    private readonly IFileService _firsService;
    //    private readonly IFileService _autoPayService;
    //    private readonly IFileService _bulkBillPaymentService;
    //    private readonly IFileService _bulkSmsService;
    //    private readonly INasRepository _nasRepository;
    //    private readonly IDbRepository _dbRepository;

    //    public ApiUploadService(Func<FileReaderTypeEnum, IFileReader> fileReader,
    //        Func<FileServiceTypeEnum, IFileService> fileService, 
    //        INasRepository nasRepository,
    //        IDbRepository dbRepository)
    //    {
    //        _txtFileReader = fileReader(FileReaderTypeEnum.TXT);
    //        _csvFileReader = fileReader(FileReaderTypeEnum.CSV);
    //        _xlsxFileReader = fileReader(FileReaderTypeEnum.XLSX);
    //        _xlsFileReader = fileReader(FileReaderTypeEnum.XLS);
    //        _firsService = fileService(FileServiceTypeEnum.Firs);
    //        _autoPayService = fileService(FileServiceTypeEnum.AutoPay);
    //        _bulkSmsService = fileService(FileServiceTypeEnum.SMS);
    //        _bulkBillPaymentService = fileService(FileServiceTypeEnum.BillPayment);
    //        _nasRepository = nasRepository;
    //        _dbRepository = dbRepository;
    //    }

    //    public async Task<BatchFileSummaryDto> GetFileSummary(string batchId)
    //    {
    //        ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));

    //        BatchFileSummaryDto batchFileSummaryDto;

    //        batchFileSummaryDto = await _bulkBillPaymentService.GetBatchUploadSummary(batchId);
           
    //        return batchFileSummaryDto;
    //    }

    //    public async Task<PagedData<BatchFileSummaryDto>> GetUserFilesSummary(string userId, PaginationFilter paginationFilter)
    //    {
    //        ArgumentGuard.NotNullOrWhiteSpace(userId, nameof(userId));

    //        var batchFileSummariesDto = new PagedData<BatchFileSummaryDto>();

    //        batchFileSummariesDto = await _bulkBillPaymentService.GetUserUploadSummaries(userId, paginationFilter);

    //        return batchFileSummariesDto;
    //    }
    //    public async Task<PagedData<BillPaymentRowStatus>> GetBillPaymentsStatus(string batchId, PaginationFilter pagination)
    //    {
    //        ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));
    //        ArgumentGuard.NotDefault(pagination.PageNumber, nameof(pagination.PageNumber));
    //        ArgumentGuard.NotDefault(pagination.PageSize, nameof(pagination.PageSize));

    //        var billPaymentStatuses = new PagedData<BillPaymentRowStatus>();

    //        try
    //        {
    //            billPaymentStatuses = await _bulkBillPaymentService.GetBillPaymentResults(batchId, pagination);
    //        }
    //        catch (AppException ex)
    //        {
    //            throw ex;
    //        }
    //        catch (Exception ex)
    //        {
    //            throw ex;
    //        }
    //        return billPaymentStatuses;
    //    }

    //    public async Task<UploadResult> UploadFileAsync(UploadOptions uploadOptions, Stream stream)
    //    {
    //        ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ContentType, nameof(uploadOptions.ContentType));
    //        ArgumentGuard.NotNullOrWhiteSpace(uploadOptions.ValidationType, nameof(uploadOptions.ValidationType));

    //        if (!uploadOptions.ValidationType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower())
    //            && !uploadOptions.ValidationType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower())
    //            && !uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WVAT.ToLower())
    //            && !uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WHT.ToLower()))                
    //            throw new AppException("Invalid Validation Type specified");

    //        if (uploadOptions.UserId == null)
    //            throw new AppException("Id cannot be null");

    //        if (uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WHT.ToLower()) 
    //            || uploadOptions.ValidationType.ToLower().Equals(GenericConstants.WVAT.ToLower()))
    //            uploadOptions.ContentType = GenericConstants.Firs;

    //        IEnumerable<Row> rows = new List<Row>();
    //        UploadResult uploadResult = new UploadResult();

    //        // uploadResult.BatchId = GenericHelpers.GenerateBatchId(uploadOptions.FileName, DateTime.Now, uploadOptions.ValidationType);
    //        var batchId = GenericHelpers.GenerateBatchId(uploadOptions.FileName, DateTime.Now, uploadOptions.ValidationType);

    //        switch (uploadOptions.FileExtension)
    //        {
    //            case "txt":
    //                rows = _txtFileReader.Read(stream);
    //                break;
    //            case "csv":
    //                rows = _csvFileReader.Read(stream);
    //                break;
    //            case "xlsx":
    //                rows = _xlsxFileReader.Read(stream);
    //                break;
    //            case "xls":
    //                rows = _xlsFileReader.Read(stream);
    //                break;
    //            default:
    //                throw new AppException("File extension not supported!.");
    //        }

    //        switch (uploadOptions.ContentType.ToLower())
    //        {
    //            case "firs":
    //                uploadResult = await _firsService.Upload(uploadOptions, rows, uploadResult);
    //                break;
    //            case "autopay":
    //                return await _autoPayService.Upload(uploadOptions, rows, uploadResult);
    //            case "sms":
    //                return await _bulkSmsService.Upload(uploadOptions, rows, uploadResult);
    //            case "billpayment":
    //                uploadResult = await _bulkBillPaymentService.Upload(uploadOptions, rows, uploadResult);
    //                break;
    //            default:
    //                throw new AppException("Content type not supported!.");
    //        }

    //        return uploadResult;
    //    }

    //    public async Task<ConfirmedBillResponse> PaymentInitiationConfirmed(string batchId, InitiatePaymentOptions initiatePaymentOptions)
    //    {
    //        try
    //        {
    //            ArgumentGuard.NotNullOrWhiteSpace(batchId, nameof(batchId));
    //            ArgumentGuard.NotNull(initiatePaymentOptions.BusinessId, nameof(initiatePaymentOptions.BusinessId));
    //            ArgumentGuard.NotNull(initiatePaymentOptions.ApprovalConfigId, nameof(initiatePaymentOptions.ApprovalConfigId));
    //            ArgumentGuard.NotNull(initiatePaymentOptions.UserId, nameof(initiatePaymentOptions.UserId));
    //            ArgumentGuard.NotNullOrWhiteSpace(initiatePaymentOptions.UserName, nameof(initiatePaymentOptions.UserName));

    //            return await _bulkBillPaymentService.PaymentInitiationConfirmed(batchId, initiatePaymentOptions);
    //        }
    //        catch(AppException ex)
    //        {
    //            throw ex;
    //        }
    //        catch(Exception ex)
    //        {
    //            throw ex;
    //        }
    //    }

    //    public async Task<string> GetFileTemplateContentAsync(string extension, MemoryStream outputStream)
    //    {
    //        string templateFileName;

    //        try
    //        {
    //            switch (extension.ToLower())
    //            {
    //                case "txt":
    //                    templateFileName = GenericConstants.BillPaymentTxtTemplate;
    //                    break;
    //                case "csv":
    //                    templateFileName = GenericConstants.BillPaymentCsvTemplate;
    //                    break;
    //                case "xlsx":
    //                    templateFileName = GenericConstants.BillPaymentXlsxTemplate;
    //                    break;
    //                case "xls":
    //                    templateFileName = GenericConstants.BillPaymentXlsTemplate;
    //                    break;
    //                default:
    //                    throw new AppException("File extension not supported!.");
    //            }
    //            return await _nasRepository.GetTemplateFileContentAsync(templateFileName, outputStream);
    //        }
    //        catch(AppException ex)
    //        {
    //            throw ex;
    //        }
    //        catch(Exception ex)
    //        {
    //            throw ex;
    //        }
    //    }

    //    public async Task<string> GetFileValidationResultAsync(string batchId, MemoryStream outputStream)
    //    {
    //        var batchFileSummary = await _dbRepository.GetBatchUploadSummary(batchId);

    //        if (batchFileSummary == null)
    //            throw new AppException($"Batch Upload Summary for BatchId: {batchId} not found", (int)HttpStatusCode.NotFound);

    //        if(string.IsNullOrWhiteSpace(batchFileSummary.NasUserValidationFile))
    //            throw new AppException($"Validation file not found for batch with Id : {batchId}", (int)HttpStatusCode.NotFound);

    //        await _nasRepository.GetUserValidationResultAsync(batchFileSummary.NasUserValidationFile, outputStream);

    //        return batchFileSummary.NasUserValidationFile;
    //    }
    }

}
