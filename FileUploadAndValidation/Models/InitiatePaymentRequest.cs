using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class InitiatePaymentRequest
    {
        public long BusinessId { get; set; }

        public long UserId { get; set; }

        public string UserName { get; set; }

        public long ApprovalConfigId { get; set; }

    }
}
