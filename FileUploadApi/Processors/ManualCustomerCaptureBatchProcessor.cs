using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation;
using FileUploadAndValidation.BillPayments;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
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

    public ManualCustomerCaptureBatchProcessor(IFileContentValidator<ManualCaptureRow, ManualCustomerCaptureContext> fileContentValidator,
                                        IRemoteFileContentValidator<ManualCaptureRow> remoteValidator,
                                        IDetailsDbRepository<ManualCaptureRow> dbRepository)
    {
        _fileContentValidator = fileContentValidator;
        this.remoteValidator = remoteValidator;
        this.dbRepository = dbRepository;
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

        var batchId = GenericHelpers.GenerateBatchId("QTB", DateTime.Now);

        var remoteValidationResult = await remoteValidator.Validate(batchId, localValidationResult.ValidRows, clientToken);

        var isBackground = remoteValidator.IsBackground();

        var finalResult = isBackground ? localValidationResult : localValidationResult.MergeResults(remoteValidationResult);

        var batch = new Batch<ManualCaptureRow>(finalResult.ValidRows, finalResult.Failures)
        {
            BatchId = batchId,
            ItemType = "manual-capture",
            ContentType = "manual-capture",
            UploadDate = DateTime.Now.ToShortDateString(),
            ModifiedDate = DateTime.Now.ToShortDateString(),
            ProductCode = "FCT-IRS",
            ProductName = "FCT-IRS",
            UserId = context.UserId,
            TransactionStatus = GenericConstants.PendingValidation,
            NameOfFile = context.FileName
            //UplodedBy =  context.UserName,           
        };

        await dbRepository.InsertAllUploadRecords(batch);

        return batch;
    }
}