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

public partial class LASGPaymentBatchProcessor : IBatchFileProcessor<LASGPaymentContext>
{
    private readonly IFileContentValidator<LASGPaymentRow, LASGPaymentContext> _fileContentValidator;
    private readonly INasRepository nasRepository;
    private readonly IDetailsDbRepository<LASGPaymentRow> dbRepository;
    private readonly IRemoteFileContentValidator<LASGPaymentRow> remoteValidator;

    public LASGPaymentBatchProcessor(IFileContentValidator<LASGPaymentRow, LASGPaymentContext> fileContentValidator,
                                        IRemoteFileContentValidator<LASGPaymentRow> remoteValidator,
                                        IDetailsDbRepository<LASGPaymentRow> dbRepository)
    {
        _fileContentValidator = fileContentValidator;
        this.remoteValidator = remoteValidator;
        this.dbRepository = dbRepository;
    }

    public async Task<BatchFileSummary> UploadAsync(IEnumerable<Row> rows, LASGPaymentContext context)
    {
        if (rows.Count() == 0) throw new AppException("No records found");

        var localValidationResult = await _fileContentValidator.Validate(rows, context);//, null);
        Console.WriteLine(JsonConvert.SerializeObject(localValidationResult));

        Console.WriteLine(JsonConvert.SerializeObject(localValidationResult));
        if (!localValidationResult.ValidRows.Any()){
            var errors = localValidationResult.Failures.SelectMany(e => e.ErrorMessages);
            throw new AppException("No valid rows");
        }

        var batchId = GenericHelpers.GenerateBatchId("QTB", DateTime.Now);

        var remoteValidationResult = await remoteValidator.Validate(batchId, localValidationResult.ValidRows);

        var finalResult = localValidationResult.MergeResults(remoteValidationResult);

        var batch = new Batch<LASGPaymentRow>(finalResult.ValidRows, finalResult.Failures)
        {
            BatchId = batchId,
            ItemType = "lasg",
            ContentType = "lasg",
            UploadDate = DateTime.Now.ToShortDateString(),
            ModifiedDate = DateTime.Now.ToShortDateString(),
            ProductCode = "LASG",
            ProductName = "LASG",
            UserId = context.UserId,
            //UplodedBy =  context.UserName,           
        };

        await dbRepository.InsertAllUploadRecords(batch);

        return batch;
    }
}