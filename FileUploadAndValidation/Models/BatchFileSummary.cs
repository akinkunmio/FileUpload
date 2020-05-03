using System;
using System.Collections.Generic;
using System.Text;

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

        public double ValidAmountSum { get; set; }

        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        public string FileName { get; set; }
    }
}
