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

        public async Task Save(UploadResult uploadResult, FileUploadRequest request)
        {

            var totalNoOfRows = uploadResult.ValidRows.Count + uploadResult.Failures.Count;
         
            await _dbRepository.InsertAllUploadRecords(new UploadSummaryDto
            {
                BatchId = uploadResult.BatchId,
                NumOfAllRecords = totalNoOfRows,
                Status = GenericConstants.PendingValidation,
                UploadDate = DateTime.Now.ToString(),
                CustomerFileName = request.FileName,
                ItemType = request.ItemType,
                ContentType = request.ContentType,
                UserId = (long)request.UserId,
                ProductName = request.ProductName,
                ProductCode = request.ProductCode
            }, uploadResult.ValidRows, uploadResult.Failures);


            FileProperty fileProperty = await _nasRepository.SaveFileToValidate(uploadResult.BatchId, request.ItemType, uploadResult.ValidRows.AsEnumerable());

            fileProperty.BusinessTin = request.BusinessTin;
            fileProperty.ContentType = request.ContentType;
            fileProperty.ItemType = request.ItemType;

            var validationResponse = await _httpService.ValidateRecords(fileProperty, request.AuthToken, uploadResult.ValidRows.Count() > 50);

            string validationResultFileName;

            if (validationResponse.ResponseData.NumOfRecords <= GenericConstants.RECORDS_SMALL_SIZE && validationResponse.ResponseData.Results.Any() && validationResponse.ResponseData.ResultMode.ToLower().Equals("json"))
            {
                var entValidatedRecordsCount = validationResponse.ResponseData.Results.Where(v => v.Status.ToLower().Equals("valid")).Count();
                
                await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
                {
                    BatchId = uploadResult.BatchId,
                    NasToValidateFile = fileProperty.Url,
                    ModifiedDate = DateTime.Now.ToString(),
                    NumOfValidRecords = entValidatedRecordsCount,
                    Status = (entValidatedRecordsCount > 0) ? GenericConstants.AwaitingInitiation : GenericConstants.NoValidRecord,
                    RowStatuses = validationResponse.ResponseData.Results,
                });

                RowStatusDtoObject validationResult = await _dbRepository.GetPaymentRowStatuses(uploadResult.BatchId, 
                    new PaginationFilter
                    {
                        PageSize = totalNoOfRows,
                        PageNumber = 1,
                        ItemType = request.ItemType,
                        ContentType = request.ContentType
                    });

                validationResultFileName = await _nasRepository.SaveValidationResultFile(uploadResult.BatchId, request.ItemType, validationResult.RowStatusDto);

                await _dbRepository.UpdateUploadSuccess(uploadResult.BatchId, validationResultFileName);
            }
        }

    }
    

}
