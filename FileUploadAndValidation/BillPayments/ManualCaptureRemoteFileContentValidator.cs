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
    public class ManualCaptureRemoteFileContentValidator : IRemoteFileContentValidator<ManualCaptureRow>
    {
        private readonly INasRepository _nasRepository;
        private readonly IHttpService _httpService;
        private bool _isBackground = true;

        public ManualCaptureRemoteFileContentValidator(INasRepository nasRepository, IHttpService httpService)
        {
            _nasRepository = nasRepository;
            _httpService = httpService;
        }

        public bool IsBackground()
        {
            return _isBackground;
        }

        public async Task<ValidationResult<ManualCaptureRow>> Validate(string requestIdentifier, IEnumerable<ManualCaptureRow> validRows, long businessId, string clientToken)
        {
            var fileProperty = await _nasRepository.SaveFileToValidate<ManualCaptureRow>(batchId: requestIdentifier, rowDetails: validRows.ToList());

            ValidationResponse validationResponse;
            try
            {
                fileProperty.ItemType = GenericConstants.ManualCapture;
                fileProperty.ContentType = GenericConstants.ManualCapture;
                fileProperty.BusinessId = businessId;
                validationResponse = await _httpService.ValidateRecords(fileProperty, clientToken, true);

            }
            catch (AppException ex)
            {
                validationResponse = new ValidationResponse
                {
                    ResponseCode = ex.StatusCode.ToString()
                };
            }

            IEnumerable<ManualCaptureRow> result = new List<ManualCaptureRow>();
            var isSuccessResponse = new[] { "200", "201", "204", "90000" }.Contains(validationResponse.ResponseCode);
            if (!isSuccessResponse)
                return RemoteValidationUtil.HandleFailureResponse<ManualCaptureRow>(validationResponse.ResponseCode);

            if (validationResponse.ResponseData.ResultMode == "json")
            {
                result = validationResponse.ResponseData.Results.Select(r => ToPaymentRow(r));
                _isBackground = false;

                return new ValidationResult<ManualCaptureRow>
                {
                    CompletionStatus = new CompletionState { Status = CompletionStateStatus.Completed },
                    ValidRows = result.Where(r => r.IsValid).ToList(),
                    Failures = result.Where(r => !r.IsValid).ToList()
                };
            }
            else
            {
                _isBackground = true;
                return new ValidationResult<ManualCaptureRow>
                {
                    CompletionStatus = new CompletionState { Status = CompletionStateStatus.Queued },
                    ValidRows = new List<ManualCaptureRow>(),
                    Failures = new List<ManualCaptureRow>()
                };
            }
        }

        private ManualCaptureRow ToPaymentRow(RowValidationStatus r)
        {
            return new ManualCaptureRow
            {
                IsValid = r.Status == "valid",
                Row = r.Row,
                ErrorMessages = new[] { r.Error },
                CustomerId = r.ExtraData
            };
        }
    }
}