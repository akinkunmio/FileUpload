using FilleUploadCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Models
{
    public class FileUploadResult
    {
        public FileUploadResult()
        {
            ValidatedRecordsRowNumber = new List<int>();
            Failures = new List<Failure>();
        }

        public string ErrorMessage { get; set; }

        public IList<int> ValidatedRecordsRowNumber { get; set; }

        public IList<Failure> Failures { get; set; }

        public int RecordsCount { get; set; }

        public class Failure
        {
            public int? RowNumber { get; set; }

            public IList<ValidationError> ValidationErrors { get; set; }

            public MappingError MappingError { get; set; }
        }
    }
}
