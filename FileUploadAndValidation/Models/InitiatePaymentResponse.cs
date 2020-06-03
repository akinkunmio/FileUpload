using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class InitiatePaymentResponse
    {
        public string ResponseCode { get; set; }
        public InitiatePaymentResponseData ResponseData { get; set; }
        public string ResponseDescription { get; set; }

    }

    public class InitiatePaymentResponseData
    {
        public object approvalStatusKey { get; set; }

        public string status { get; set; }

        public string verdict { get; set; }

        public string levelIdNext { get; set; }

        public string noOfApprovals { get; set; }

        public string lastUpdate { get; set; }

        public object userIds { get; set; }

        public string nextUserIds { get; set; }
    }

}
