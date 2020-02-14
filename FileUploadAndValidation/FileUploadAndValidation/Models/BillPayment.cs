using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class BillPayment
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public int Amount { get; set; }

        public string BatchId { get; set; }

        public int RowNumber { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

        public string CreatedDate { get; set; }

        public string ModifiedDate { get; set; }
    }
}
