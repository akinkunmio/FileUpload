using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class NasBillPaymentDto
    {
        public double amount { get; set; }
        public string customer_id { get; set; }
        public int row { get; set; }
        public string item_code { get; set; }
        public string product_code { get; set; }
    }
}
