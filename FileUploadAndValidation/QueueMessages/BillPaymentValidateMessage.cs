using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadAndValidation.QueueMessages
{
    public class BillPaymentValidateMessage : IMessage
    {
        public BillPaymentValidateMessage(string batchId, string resultLocation, DateTime createdAt)
        {
            BatchId = batchId;
            ResultLocation = resultLocation;
            CreatedAt = createdAt;
        }
        public string BatchId { get; set; }

        public string ResultLocation { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
