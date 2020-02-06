using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class ValidationResponse
    {
        public int numOfRecords { get; set; }
        public string resultsMode { get; set; }
        public List<ValidateStatus> results { get; set; }
    }

    public class ValidateStatus
    {
        public int row { get; set; }
        public string status { get; set; }
        public string error { get; set; }
    }

    public class FileProperties
    {
        public string fileLocation { get; set; }
        public string fileName { get; set; }
    }
}
