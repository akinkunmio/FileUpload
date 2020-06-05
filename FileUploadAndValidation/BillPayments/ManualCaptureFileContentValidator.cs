using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadApi.Services;
using FilleUploadCore.FileReaders;

namespace FileUploadAndValidation.BillPayments
{

    public class ManualCaptureFileContentValidator : IFileContentValidator<ManualCaptureRow, ManualCustomerCaptureContext>
    {
        private ManualCaptureRowConfig config;

        public bool CanProcess(string contentType)
        {
            return contentType.ToUpper() == "MANUAL_CAPTURE";
        }

        public async Task<ValidationResult<ManualCaptureRow>> Validate(IEnumerable<Row> rows, ManualCustomerCaptureContext context)
        {
            await Task.CompletedTask;

            var processedRows = new List<ManualCaptureRow>();
            var validationConfig = context.Configuration ?? new ManualCaptureRowConfig();

            foreach(Row row in rows) {
                processedRows.Add(new ManualCaptureRow(row, validationConfig));
            }

            return new ValidationResult<ManualCaptureRow> {
                ValidRows = processedRows.Where(r => r.IsValid).ToList(),
                Failures = processedRows.Where(r => !r.IsValid).ToList()
            };
        }
    }
}