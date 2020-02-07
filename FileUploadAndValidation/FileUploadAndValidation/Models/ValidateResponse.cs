using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class ValidationResponse
    {
        public int NumbOfRecords { get; set; }
        public string ResultsMode { get; set; }
        public List<ValidateStatus> Results { get; set; }
    }

    public class ValidateStatus
    {
        public int Row { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
    }

    public class FileProperties
    {
        public string FileLocation { get; set; }
        public string FileName { get; set; }
    }
}
