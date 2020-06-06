using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation.Repository;
using FileUploadApi.Services;

namespace FileUploadAndValidation.BillPayments
{
    public class LASGPaymentRemoteFileContentValidator : IRemoteFileContentValidator<LASGPaymentRow>
    {
        private readonly INasRepository _nasRepository;

        public LASGPaymentRemoteFileContentValidator(INasRepository nasRepository)
        {
            _nasRepository = nasRepository;
        }

        public bool IsBackground()
        {
            throw new System.NotImplementedException();
        }

        public async Task<ValidationResult<LASGPaymentRow>> Validate(string requestIdentifier, IEnumerable<LASGPaymentRow> validRows)
        {
            await _nasRepository.SaveFileToValidate<LASGPaymentRow>(batchId: requestIdentifier, rowDetails: validRows.ToList());
            
            return new ValidationResult<LASGPaymentRow> {
                ValidRows = validRows.ToList(),
                Failures = new List<LASGPaymentRow>()
            };
        }

        public Task<ValidationResult<LASGPaymentRow>> Validate(string requestIdentifier, IEnumerable<LASGPaymentRow> validRows, string clientToken = "")
        {
            throw new System.NotImplementedException();
        }
    }
}