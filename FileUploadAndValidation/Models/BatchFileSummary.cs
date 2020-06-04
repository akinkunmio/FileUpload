using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileUploadApi.Services;
using Newtonsoft.Json;

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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NasToValidateFile { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NasRawFile { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NasConfirmedFile { get; set; }

        public string ContentType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NasUserValidationFile { get; set; }

        public decimal ValidAmountSum { get; set; }

        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NameOfFile { get; set; }

        public bool? UploadSuccessful { get; set; }
    }

    public class Batch<T> : BatchFileSummary where T:ValidatedRow
    {
        public Batch(IList<T> validRows, IList<T> failures)
        {
            this.ValidRows = validRows;
            this.FailedRows = failures;
            this.Rows = validRows.Concat(failures);

            NumOfValidRecords = validRows.Count;
            NumOfRecords = NumOfValidRecords + failures.Count;
            ValidAmountSum = validRows.Sum(r => r.Amount);
        }

        public IList<T> ValidRows { get; private set; }
        public IList<T> FailedRows { get; private set; }
        public IEnumerable<T> Rows { get; private set; }
        public long UserId { get; private set; }
    }

}
