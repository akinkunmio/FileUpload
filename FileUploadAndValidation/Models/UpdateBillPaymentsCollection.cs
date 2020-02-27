using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class UpdateValidationResponseModel
    {
        public string BatchId { get; set; }

        public string Status { get; set; }

        public int NumOfValidRecords { get; set; }

        public string ModifiedDate { get; set; }

        public string NasToValidateFile { get; set; }

        public List<RowValidationStatus> RowStatuses { get; set; }
    }
}
