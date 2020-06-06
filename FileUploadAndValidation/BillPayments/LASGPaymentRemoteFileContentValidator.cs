using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Services;

namespace FileUploadAndValidation.BillPayments
{
    public class LASGPaymentRemoteFileContentValidator : IRemoteFileContentValidator<LASGPaymentRow>
    {
        private readonly INasRepository _nasRepository;
        private readonly IHttpService _httpService;
        private bool IsBackground = true;

        public LASGPaymentRemoteFileContentValidator(INasRepository nasRepository,
            IHttpService httpService)
        {
            _nasRepository = nasRepository;
            _httpService = httpService;
        }
        public async Task<ValidationResult<LASGPaymentRow>> Validate(string requestIdentifier, IEnumerable<LASGPaymentRow> validRows)
        {
            var fileProperty = await _nasRepository.SaveFileToValidate<LASGPaymentRow>(batchId: requestIdentifier, rowDetails: validRows.ToList());
            var validationResponse = await _httpService.ValidateRecords(fileProperty, "", true);
            IEnumerable<LASGPaymentRow> result = new List<LASGPaymentRow>();
            if(validationResponse.ResponseCode == "200") {
                if(validationResponse.ResponseData.ResultMode == "json") 
                {
                    result = validationResponse.ResponseData.Results.Select(r => ToPaymentRow(r));
                    IsBackground = false;
                }
                else {
                    IsBackground = true;
                }
            }
            
            return new ValidationResult<LASGPaymentRow> {
                ValidRows = result.Where(r => r.IsValid).ToList(),
                Failures =  result.Where(r => !r.IsValid).ToList()
            };
        }

        private LASGPaymentRow ToPaymentRow(RowValidationStatus r)
        {
            return new LASGPaymentRow
            {
                IsValid = r.Status == "valid",
                Index = r.Row,
                ErrorMessages = new[] { r.Error },
                CustomerId = r.ExtraData
            };
        }
    }
}