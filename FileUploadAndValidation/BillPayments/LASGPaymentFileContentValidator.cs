using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadApi.Services;
using FilleUploadCore.FileReaders;

namespace FileUploadAndValidation.BillPayments
{
    public class LASGPaymentFileContentValidator : IFileContentValidator<LASGPaymentRow, LASGPaymentContext>
    {
        public bool CanProcess(string contentType)
        {
            return contentType.ToUpper() == "LASG";
        }

        public async Task<ValidationResult<LASGPaymentRow>> Validate(IEnumerable<Row> rows, LASGPaymentContext context)
        {
            await Task.CompletedTask;

            var processedRows = new List<LASGPaymentRow>();

            foreach(Row row in rows) {
                processedRows.Add(new LASGPaymentRow(row));
            }

            return new ValidationResult<LASGPaymentRow> {
                ValidRows = processedRows.Where(r => r.IsValid).ToList(),
                Failures = processedRows.Where(r => !r.IsValid).ToList()
            };
        }
    }
}