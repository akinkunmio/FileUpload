using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation;
using FileUploadAndValidation.BillPayments;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.Utils;
using FileUploadApi;
using FileUploadApi.Processors;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using Newtonsoft.Json;

public partial class ManualCustomerCaptureBatchProcessor : IBatchFileProcessor<ManualCustomerCaptureContext>
{
    private readonly IFileContentValidator<ManualCaptureRow, ManualCustomerCaptureContext> _fileContentValidator;
    private readonly INasRepository nasRepository;
    private readonly IDetailsDbRepository<ManualCaptureRow> dbRepository;
    private readonly IRemoteFileContentValidator<ManualCaptureRow> remoteValidator;
    private readonly IAppConfig _appConfig;
    private readonly ApprovalUtil _approvalUtil;

    public ManualCustomerCaptureBatchProcessor(IFileContentValidator<ManualCaptureRow, ManualCustomerCaptureContext> fileContentValidator,
                                        IRemoteFileContentValidator<ManualCaptureRow> remoteValidator, IAppConfig appConfig,
                                        IDetailsDbRepository<ManualCaptureRow> dbRepository, ApprovalUtil approvalUtil)
    {
        _fileContentValidator = fileContentValidator;
        this.remoteValidator = remoteValidator;
        this.dbRepository = dbRepository;
        _appConfig = appConfig;
        _approvalUtil = approvalUtil;
    }

    public async Task<BatchFileSummary> UploadAsync(IEnumerable<Row> rows, ManualCustomerCaptureContext context, string clientToken)
    {
        if (rows.Count() == 0) throw new AppException("No records found");

        var localValidationResult = await _fileContentValidator.Validate(rows, context);//, null);

        Console.WriteLine(JsonConvert.SerializeObject(localValidationResult));
        if (!localValidationResult.ValidRows.Any()){
            var errors = localValidationResult.Failures.SelectMany(e => e.ErrorMessages);
            throw new AppException("No valid rows");
        }
        var totalAmount = (long)localValidationResult.ValidRows.Select(p => p.Amount).ToList().Sum(s => s);
        
        var approvalResponse = await _approvalUtil.GetApprovalConfiguration(context.BusinessId, context.UserId, totalAmount);

        if (!approvalResponse)
            throw new AppException("User is not enabled to upload a file", 400);


        var batchId = GenericHelpers.GenerateBatchId($"QTB_{context.ContentType.ToUpper()}", DateTime.Now);

        var remoteValidationResult = await remoteValidator.Validate(batchId, localValidationResult.ValidRows, context.BusinessId, clientToken);

        var finalResult = localValidationResult.MergeResults(remoteValidationResult);

        var batch = new Batch<ManualCaptureRow>(finalResult.ValidRows, finalResult.Failures)
        {
            BatchId = batchId,
            ItemType = GenericConstants.ManualCapture,
            ContentType = GenericConstants.ManualCapture,
            UploadDate = DateTime.Now.ToString(),
            ModifiedDate = DateTime.Now.ToString(),
            ProductCode = context.ProductCode,
            ProductName = GenericConstants.FctIrs.Equals(context.ContentType) ? "FCT-IRS" : context.ProductName,
            UserId = context.UserId,
            TransactionStatus = GenericConstants.PendingValidation,
            NameOfFile = string.Empty,
            Status = RemoteValidationUtil.GetStatusFromRemoteResponseCode(remoteValidationResult.CompletionStatus.Status),
        };

        await dbRepository.InsertAllUploadRecords(batch);

        return batch;
    }
}