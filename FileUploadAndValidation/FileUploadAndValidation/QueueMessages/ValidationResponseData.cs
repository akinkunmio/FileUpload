using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.QueueMessages
{
    public class ValidationResponseData
    {
        public string RequestId { get; set; }
        public string ResultLocation { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
