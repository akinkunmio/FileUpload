using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Services;

namespace FileUploadAndValidation.BillPayments
{
    public class ManualCaptureRemoteFileContentValidator : IRemoteFileContentValidator<ManualCaptureRow>
    {
        private readonly INasRepository _nasRepository;
        private readonly IHttpService _httpService;

        public ManualCaptureRemoteFileContentValidator(INasRepository nasRepository, IHttpService httpService)
        {
            _nasRepository = nasRepository;
            _httpService = httpService;
        }

        public bool IsBackground()
        {
            return true;
        }

        public async Task<ValidationResult<ManualCaptureRow>> Validate(string requestIdentifier, IEnumerable<ManualCaptureRow> validRows, string clientToken)
        {
            var result = await _nasRepository.SaveFileToValidate<ManualCaptureRow>(batchId: requestIdentifier, rowDetails: validRows.ToList());

            result.ItemType = GenericConstants.ManualCapture;
            result.ContentType = GenericConstants.ManualCapture;
            var remoteResponse = await _httpService.ValidateRecords(result, clientToken, true);

            return new ValidationResult<ManualCaptureRow> {
                CompletionStatus = new CompletionState { Status = CompletionStateStatus.Queued },
                ValidRows = validRows.ToList(),
                Failures = new List<ManualCaptureRow>()
            };
        }
    }
}