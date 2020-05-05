using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Repository
{
    public class BatchRepository : IBatchRepository
    {
        private readonly IHttpService _httpService;
        private readonly INasRepository _nasRepository;
        private readonly IDbRepository _dbRepository;

        public BatchRepository(IDbRepository dbRepository,
            INasRepository nasRepository,
            IHttpService httpService)
        {
            _dbRepository = dbRepository;
            _nasRepository = nasRepository;
            _httpService = httpService;
        }

        public async Task Save(string batchId, FileUploadRequest request, IList<RowDetail> validRows, IList<Failure> failures)
        {
            var totalNoOfRows = validRows.Count + failures.Count;
         
            await _dbRepository.InsertAllUploadRecords(new UploadSummaryDto
            {
                BatchId = batchId,
                NumOfAllRecords = totalNoOfRows,
                Status = GenericConstants.PendingValidation,
                UploadDate = DateTime.Now.ToString(),
                CustomerFileName = request.FileName,
                ItemType = request.ItemType,
                ContentType = request.ContentType,
                UserId = (long)request.UserId
            }, validRows, failures);


            FileProperty fileProperty = await _nasRepository.SaveFileToValidate(batchId, request.ItemType, validRows.AsEnumerable());

            fileProperty.BusinessTin = request.BusinessTin ?? "";
            fileProperty.ContentType = request.ContentType;
            fileProperty.ItemType = request.ItemType;

            var validationResponse = await _httpService.ValidateBillRecords(fileProperty, request.AuthToken, validRows.Count() > 50);

            string validationResultFileName;

            if (validationResponse.Data.NumOfRecords <= GenericConstants.RECORDS_SMALL_SIZE && validationResponse.Data.Results.Any() && validationResponse.Data.ResultMode.ToLower().Equals("json"))
            {
                var entValidatedRecordsCount = validationResponse.Data.Results.Where(v => v.Status.ToLower().Equals("valid")).Count();
                
                await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
                {
                    BatchId = batchId,
                    NasToValidateFile = fileProperty.Url,
                    ModifiedDate = DateTime.Now.ToString(),
                    NumOfValidRecords = entValidatedRecordsCount,
                    Status = (entValidatedRecordsCount > 0) ? GenericConstants.AwaitingInitiation : GenericConstants.NoValidRecord,
                    RowStatuses = validationResponse.Data.Results,
                });

                RowStatusDtoObject validationResult = await _dbRepository.GetPaymentRowStatuses(batchId, 
                    new PaginationFilter
                    {
                        PageSize = totalNoOfRows,
                        PageNumber = 1,
                        ItemType = request.ItemType,
                        ContentType = request.ContentType
                    });

                validationResultFileName = await _nasRepository.SaveValidationResultFile(batchId, request.ItemType, validationResult.RowStatusDto);

                await _dbRepository.UpdateUploadSuccess(batchId, validationResultFileName);
            }
        }

    }
    public interface IBatchRepository
    {
        Task Save(string batchId, FileUploadRequest request, IList<RowDetail> validRows, IList<Failure> failures);
    }

}
