using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class BillPaymentStatus
    {
        public int RowNumber { get; set; }

        public string ErrorResponse { get; set; }

        public string ReferenceId { get; set; }

        public string Status { get; set; }

        public string UploadDate { get; set; }
    }
}
