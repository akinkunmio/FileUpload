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

        public int NumOfAllRecords { get; set; }

        public int NumOfValidRecords { get; set; }

        public string Status { get; set; }

        public string ItemType { get; set; }

        public string ContentType { get; set; }

        public string FileName { get; set; }
    }
}
