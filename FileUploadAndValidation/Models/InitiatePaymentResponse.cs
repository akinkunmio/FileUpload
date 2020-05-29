using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class InitiatePaymentResponse
    {
        public string ResponseCode { get; set; }
        public object ResponseData { get; set; }
        public string ResponseDescription { get; set; }

    }

}
