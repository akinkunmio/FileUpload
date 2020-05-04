using FileUploadAndValidation;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public class AutoPayBatchFileProcessor : IBatchFileProcessor<AutoPayUploadContext>
    {
        private readonly IFileContentValidator<AutoPayRow> _fileContentValidator;
        private readonly INasRepository nasRepository;
        private readonly IDetailsDbRepository<AutoPayRow> dbRepository;
        private readonly AutoPayFileContentRemoteValidator remoteValidator;

        public AutoPayBatchFileProcessor(IFileContentValidator<AutoPayRow> fileContentValidator,
                                        AutoPayFileContentRemoteValidator remoteValidator,
                                        INasRepository nasRepository,
                                        IDetailsDbRepository<AutoPayRow> dbRepository)
        {
            _fileContentValidator = fileContentValidator;
            this.remoteValidator = remoteValidator;
            this.nasRepository = nasRepository;
            this.dbRepository = dbRepository;
        }
        public async Task<BatchFileSummary> UploadAsync(IEnumerable<Row> rows, AutoPayUploadContext context)
        {
            if(rows.Count() == 0)
                throw new AppException("No records found");

            var localValidationResult = await _fileContentValidator.Validate(rows);//, null);
            if(!localValidationResult.ValidRows.Any())
                throw new AppException("No valid rows");

            var batchId = GenericHelpers.GenerateBatchId("QTB", DateTime.Now);

            FileProperty fileProperty = await nasRepository.SaveFileToValidate(batchId, localValidationResult.ValidRows);

            var remoteValidationResult = await _fileContentValidator.ValidateRemote(localValidationResult.ValidRows);

            var finalResult = MergeResults(remoteValidationResult, localValidationResult);

            var batch = new BatchFileSummary() { BatchId = batchId };
            batch.SetResults(finalResult);

            await dbRepository.InsertAllUploadRecords(new UploadSummaryDto{
                BatchId = batchId
            }, finalResult.ValidRows, finalResult.Failures);
                
            // validator needs the rows and the context (security, request)
            // validator needs the rules to validate (this can be inside the row)
            // validator needs remote service to do additional validation
            // validator needs nas-service-repo to do remote

            return batch;
        }

        private ValidationResult<AutoPayRow> MergeResults(ValidationResult<AutoPayRow> remoteValidationResult, ValidationResult<AutoPayRow> localValidationResult)
        {
            remoteValidationResult.Failures.ToList().AddRange(localValidationResult.Failures);
            return remoteValidationResult;
        }
    }
    
   
    
}