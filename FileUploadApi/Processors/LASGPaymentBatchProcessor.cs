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

public partial class LASGPaymentBatchProcessor : IBatchFileProcessor<LASGPaymentContext>
{
    private readonly IFileContentValidator<LASGPaymentRow, LASGPaymentContext> _fileContentValidator;
    private readonly INasRepository nasRepository;
    private readonly IDetailsDbRepository<LASGPaymentRow> dbRepository;
    private readonly IRemoteFileContentValidator<LASGPaymentRow> remoteValidator;
    private readonly ApprovalUtil _approvalUtil;

    public LASGPaymentBatchProcessor(IFileContentValidator<LASGPaymentRow, LASGPaymentContext> fileContentValidator,
                                        IRemoteFileContentValidator<LASGPaymentRow> remoteValidator,
                                        IDetailsDbRepository<LASGPaymentRow> dbRepository, ApprovalUtil approvalUtil)
    {
        _fileContentValidator = fileContentValidator;
        this.remoteValidator = remoteValidator;
        this.dbRepository = dbRepository;
        _approvalUtil = approvalUtil;
    }

    public async Task<BatchFileSummary> UploadAsync(IEnumerable<Row> rows, LASGPaymentContext context, string token = "")
    {
        if (rows.Count() == 0) throw new AppException("No records found");

        var localValidationResult = await _fileContentValidator.Validate(rows, context);//, null);

        // Console.WriteLine(JsonConvert.SerializeObject(localValidationResult));
        
        if (!localValidationResult.ValidRows.Any()){
            var errors = localValidationResult.Failures.SelectMany(e => e.ErrorMessages);
            throw new AppException("No valid rows");
        }

        var totalAmount = (long)localValidationResult.ValidRows.Select(p => p.Amount).ToList().Sum(s => s);

        var approvalResponse = await _approvalUtil.GetApprovalConfiguration(context.BusinessId, context.UserId, totalAmount);

        if (!approvalResponse)
            throw new AppException("User is not enabled to upload a file", 400);

        var batchId = GenericHelpers.GenerateBatchId("QTB_LASG", DateTime.Now);

        var remoteValidationResult = await remoteValidator.Validate(batchId, localValidationResult.ValidRows, token);

        var finalResult = localValidationResult.MergeResults(remoteValidationResult);

        var batch = new Batch<LASGPaymentRow>(finalResult.ValidRows, finalResult.Failures)
        {
            BatchId = batchId,
            ItemType = GenericConstants.Lasg,
            ContentType = GenericConstants.Lasg,
            UploadDate = DateTime.Now.ToString(),
            ModifiedDate = DateTime.Now.ToString(),
            ProductCode = context.ProductCode,
            ProductName = context.ProductName,
            UserId = context.UserId,
            TransactionStatus = GenericConstants.PendingValidation,
            UploadSuccessful = true,
            Status = RemoteValidationUtil.GetStatusFromRemoteResponseCode(remoteValidationResult.CompletionStatus.Status),
            ErrorMessage = remoteValidationResult.CompletionStatus.ErrorMessage           
        };

        await dbRepository.InsertAllUploadRecords(batch);

        return batch;
    }    
}