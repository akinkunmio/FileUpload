using FilleUploadCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{

    public class UploadResult
    {
        public UploadResult()
        {
            ValidRows = new List<int>();
            Failures = new List<Failure>();
        }

        public string ErrorMessage { get; set; }

        public string BatchId { get; set; }

        public IList<int> ValidRows { get; set; }

        public IList<Failure> Failures { get; set; }

        public int RowsCount { get; set; }

        public class Failure
        {
            public int? RowNumber { get; set; }

            public IList<ValidationError> ColumnValidationErrors { get; set; }
        }
    }

}
