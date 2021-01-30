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
    public class SingleTaxBatchRepository : IBatchRepository
    {
        private readonly IHttpService _httpService;
        private readonly INasRepository _nasRepository;
        private readonly IDbRepository _dbRepository;

        public SingleTaxBatchRepository(IDbRepository dbRepository,
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
                BusinessId = (long)request.BusinessId,
                ProductName = request.ProductName,
                ProductCode = request.ProductCode
            }, uploadResult.ValidRows, uploadResult.Failures);


            FileProperty fileProperty = await _nasRepository
                                                    .SaveFileToValidate(uploadResult.BatchId,
                                                    request.ContentType,
                                                    request.ItemType,
                                                    uploadResult.ValidRows, uploadResult.ValidRows.FirstOrDefault().TaxType);

            fileProperty.ContentType = request.ContentType;
            fileProperty.ItemType = request.ItemType;
            fileProperty.BusinessId = request.BusinessId == null ? 0 : Convert.ToInt64(request.BusinessId);
            fileProperty.AdditionalData = uploadResult.ValidRows.FirstOrDefault().TaxType;

            await _httpService.ValidateRecords(fileProperty,
                request.AuthToken);
        }
    }
}
