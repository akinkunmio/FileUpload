using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.QueueMessages
{
    public class BillPaymentConfirmedMessage : IMessage
    {
        public BillPaymentConfirmedMessage(string fileName, string batchId, DateTime createdAt)
        {
            FileName = fileName;
            BatchId = batchId;
            CreatedAt = createdAt;
        }
        public string FileName { get; set; }
        public string BatchId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
