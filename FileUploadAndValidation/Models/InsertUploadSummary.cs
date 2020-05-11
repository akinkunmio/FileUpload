using FileUploadAndValidation.Models;
using System;

namespace FileUploadApi
{
    public class UploadSummaryDto
    {
        public string BatchId { get; set; }

        public string UploadDate { get; set; }

        public int NumOfAllRecords { get; set; }

        public string Status { get; set; }

        public string CustomerFileName { get; set; }

        public string NasRawFile { get; set; }

        public string ItemType { get; set; }

        public string ContentType { get; set; }
        
        public long UserId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string FileName { get; set; }
    }
}