using FileUploadAndValidation.Models;
using System;

namespace FileUploadApi
{
    public class BatchFileSummary
    {
        public int Id { get; set; }

        public string BatchId { get; set; }

        public string UploadDate { get; set; }

        public int NumOfAllRecords { get; set; }

        public int NumOfValidRecords { get; set; }

        public string Status { get; set; }

        public string UploadedBy { get; set; }

        public string CustomerFileName { get; set; }

        public string NasValidatedFileName { get; set; }

        public string NasAuthorizedFileName { get; set; }

        public string NasConfirmedFileName { get; set; }

        public string ItemType { get; set; }

        public string ContentType { get; set; }
    }
}