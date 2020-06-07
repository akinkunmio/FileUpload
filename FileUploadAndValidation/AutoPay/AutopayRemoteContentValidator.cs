using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadApi.Services;

namespace FileUploadAndValidation
{
    public class AutoPayRemoteFileContentValidator : IRemoteFileContentValidator<AutoPayRow>
    {
        public bool IsBackground()
        {
            throw new System.NotImplementedException();
        }

        public async Task<ValidationResult<AutoPayRow>> Validate(string requestIdentifier, IEnumerable<AutoPayRow> validRows)
        {
            await Task.CompletedTask;
            return new ValidationResult<AutoPayRow> {
                ValidRows = validRows.ToList(),
                Failures = new List<AutoPayRow>()
            };
        }

        public Task<ValidationResult<AutoPayRow>> Validate(string requestIdentifier, IEnumerable<AutoPayRow> validRows, string clientToken = "")
        {
            throw new System.NotImplementedException();
        }
    }
}

//-- Consolidated
//Payment Reference, Beneficiary Code, Beneficiary Name,Account Number, Account Type, CBN Code, Is CashCard, Narration, Amount, Email Address, Currency Code
//-- Not Consolidated
//Payment Reference, Payment Type, Beneficiary Code, Payment Date, Narration, Beneficiary Name, CBN Code, Account Number, Account Type, Amount, Currency Code

//C:\interswitch\paydirect-firs\firs\paydirect.firs\paydirect\firs\services\FIRSCustomService.cs
//C:\interswitch\paydirect-firs\firs\firs\schema_migrations\1415792991_assessment_payment_schedule.sql