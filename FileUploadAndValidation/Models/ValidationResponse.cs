using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class ValidationResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public ValidationData ResponseData { get; set; }
    }

    public class ValidationData
    {
        public int NumOfRecords { get; set; }
        public string ResultMode { get; set; }
        public List<RowValidationStatus> Results { get; set; }
    }

    public class RowValidationStatus
    {
        public int Row { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public string ExtraData { get; set;}
        public string WebGuid { get; set;}
        public string CustomerName { get; set;}
        public decimal Surcharge { get; set;}
        public decimal TransactionConvenienceFee { get; set;}
        public decimal BatchConvenienceFee { get; set;}
    }

    public class FileProperty
    {
        public int DataStore { get; set; }
        public string Url { get; set; }
        public string BatchId { get; set; }
        public string BusinessTin { get; set; }
        public string ItemType { get; set; }
        public string ContentType { get; set; }
    }

   
}
