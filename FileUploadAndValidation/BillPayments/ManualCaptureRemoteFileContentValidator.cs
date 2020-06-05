using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation.Repository;
using FileUploadApi.Services;

namespace FileUploadAndValidation.BillPayments
{
    public class ManualCaptureRemoteFileContentValidator : IRemoteFileContentValidator<ManualCaptureRow>
    {
        private readonly INasRepository _nasRepository;

        public ManualCaptureRemoteFileContentValidator(INasRepository nasRepository)
        {
            _nasRepository = nasRepository;
        }
        public async Task<ValidationResult<ManualCaptureRow>> Validate(string requestIdentifier, IEnumerable<ManualCaptureRow> validRows)
        {
            await _nasRepository.SaveFileToValidate<ManualCaptureRow>(batchId: requestIdentifier, rowDetails: validRows.ToList());
            
            return new ValidationResult<ManualCaptureRow> {
                ValidRows = validRows.ToList(),
                Failures = new List<ManualCaptureRow>()
            };
        }
    }
}