using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadApi.Services;
using FilleUploadCore.FileReaders;

namespace FileUploadAndValidation
{
    public class AutoPayFileContentValidator : IFileContentValidator<AutoPayRow, AutoPayUploadContext>
    {
        public bool CanProcess(string contentType)
        {
            return contentType == GenericConstants.Autopay;
        }

        public async Task<ValidationResult<AutoPayRow>> Validate(IEnumerable<Row> rows, AutoPayUploadContext context)
        {
            await Task.CompletedTask;

            List<AutoPayRow> processedRows = new List<AutoPayRow>();
            foreach(Row row in rows) {
                processedRows.Add(new AutoPayRow(row));
            }

            return new ValidationResult<AutoPayRow> {
                ValidRows = processedRows.Where(r => r.IsValid).ToList(),
                Failures = processedRows.Where(r => !r.IsValid).ToList()
            };
        }
    }
}

//-- Consolidated
//Payment Reference, Beneficiary Code, Beneficiary Name,Account Number, Account Type, CBN Code, Is CashCard, Narration, Amount, Email Address, Currency Code
//-- Not Consolidated
//Payment Reference, Payment Type, Beneficiary Code, Payment Date, Narration, Beneficiary Name, CBN Code, Account Number, Account Type, Amount, Currency Code

//C:\interswitch\paydirect-firs\firs\paydirect.firs\paydirect\firs\services\FIRSCustomService.cs
//C:\interswitch\paydirect-firs\firs\firs\schema_migrations\1415792991_assessment_payment_schedule.sql