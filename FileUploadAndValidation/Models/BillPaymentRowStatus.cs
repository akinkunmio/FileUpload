using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class BillPaymentRowStatus
    {
        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }
    }

    public class BillPaymentRowStatusDto
    {
        public string Error { get; set; }

        public string RowStatus { get; set; }

        public int RowNum { get; set; }
    }
}
