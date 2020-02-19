using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class NasBillPaymentDto
    {
        public double Amount { get; set; }
        public string CustomerId { get; set; }
        public int Row { get; set; }
        public string ItemCode { get; set; }
        public string ProductCode { get; set; }
    }
}
