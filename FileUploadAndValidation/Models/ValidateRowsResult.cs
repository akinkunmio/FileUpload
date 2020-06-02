using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class ValidateRowModel
    {
        public bool isValid { get; set; }
        public RowDetail Valid { get; set; }
        public Failure Failure { get; set; }
    }
    public class ValidateRowsResult
    {
        public List<Failure> Failures { get; set; }
        public List<RowDetail> ValidRows { get; set; }
    }
}
