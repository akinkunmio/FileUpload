using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadApi.Models
{
    public class BatchFileSummaryDto
    {
        public string BatchId { get; set; }

        public string UploadDate { get; set; }

        public int RecordsCount { get; set; }

        public int ValidRecordsCount { get; set; }

        public string Status { get; set; }

        public string ItemType { get; set; }

        public string ContentType { get; set; }

        public string FileName { get; set; }
        public decimal ValidAmountSum { get; set; }
        public int InvalidRecordsCount { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }
}
