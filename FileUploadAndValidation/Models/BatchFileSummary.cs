using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileUploadApi.Services;

namespace FileUploadAndValidation.Models
{
    public class BatchFileSummary
    {
        public string BatchId { get; set; }

        public string TransactionStatus { get; set; }

        public string ItemType { get; set; }

        public int NumOfRecords { get; set; }

        public string UploadDate { get; set; }

        public string ModifiedDate { get; set; }

        public int NumOfValidRecords { get; set; }

        public string NasToValidateFile { get; set; }

        public string NasRawFile { get; set; }

        public string NasConfirmedFile { get; set; }

        public string ContentType { get; set; }

        public string NasUserValidationFile { get; set; }

        public decimal ValidAmountSum { get; set; }

        public void SetResults(ValidationResult<AutoPayRow> finalResult)
        {
            int countOfValid = finalResult.ValidRows.Count;
            int countOfFailed = finalResult.Failures.Count;

            NumOfRecords = countOfFailed + countOfValid;
            NumOfValidRecords = countOfValid;
            ValidAmountSum = finalResult.ValidRows.Select(r => r.Amount).Sum();
        }
    }
}
