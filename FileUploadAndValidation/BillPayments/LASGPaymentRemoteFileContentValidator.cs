using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;

namespace FileUploadAndValidation.BillPayments
{
    public class LASGPaymentRemoteFileContentValidator : IRemoteFileContentValidator<LASGPaymentRow>
    {
        private readonly INasRepository _nasRepository;
        private readonly IHttpService _httpService;
        private bool _isBackground = true;

        public LASGPaymentRemoteFileContentValidator(INasRepository nasRepository,
            IHttpService httpService)
        {
            _nasRepository = nasRepository;
            _httpService = httpService;
        }

        public bool IsBackground()
        {
            return _isBackground;
        }

        public async Task<ValidationResult<LASGPaymentRow>> Validate(string requestIdentifier, IEnumerable<LASGPaymentRow> validRows, string clientToken = "")
        {
            var fileProperty = await _nasRepository.SaveFileToValidate<LASGPaymentRow>(batchId: requestIdentifier, rowDetails: validRows.ToList());
            ValidationResponse validationResponse;
            try
            {
                fileProperty.ContentType = GenericConstants.Lasg;
                fileProperty.ItemType = GenericConstants.Lasg;
                validationResponse = await _httpService.ValidateRecords(fileProperty, clientToken, true);
            }
            catch (AppException ex)
            {
                validationResponse = new ValidationResponse
                {
                    ResponseCode = ex.StatusCode.ToString()
                };
            }

            IEnumerable<LASGPaymentRow> result = new List<LASGPaymentRow>();
            var isSuccessResponse = new[] { "200", "201", "204", "90000" }.Contains(validationResponse.ResponseCode);
            if (!isSuccessResponse)
                return RemoteValidationUtil.HandleFailureResponse<LASGPaymentRow>(validationResponse.ResponseCode);

            if (validationResponse.ResponseData.ResultMode == "json")
            {
                result = validationResponse.ResponseData.Results.Select(r => ToPaymentRow(r));
                _isBackground = false;

                return new ValidationResult<LASGPaymentRow>
                {
                    CompletionStatus = new CompletionState{ Status = CompletionStateStatus.Completed },
                    ValidRows = result.Where(r => r.IsValid).ToList(),
                    Failures = result.Where(r => !r.IsValid).ToList()
                };
            }
            else
            {
                _isBackground = true;
                return new ValidationResult<LASGPaymentRow> {
                    CompletionStatus = new CompletionState{ Status = CompletionStateStatus.Queued },
                    ValidRows = new List<LASGPaymentRow>(),
                    Failures = new List<LASGPaymentRow>()
                };
            }  
        }
        private LASGPaymentRow ToPaymentRow(RowValidationStatus r)
        {
            return new LASGPaymentRow
            {
                IsValid = r.Status == "valid",
                Row = r.Row,
                ErrorMessages = new[] { r.Error },
                CustomerId = r.ExtraData
            };
        }
    }

}