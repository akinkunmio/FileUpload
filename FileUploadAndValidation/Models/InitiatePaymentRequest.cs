using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class InitiatePaymentRequest
    {
        public int BusinessId { get; set; }

        public long UserId { get; set; }

        public string UserName { get; set; }

        public int ApprovalConfigId { get; set; }

    }
}
