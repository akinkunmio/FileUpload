using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class ValidateRowModel
    {
        public bool IsValid { get; set; }
        public Failure Failure { get; set; }
    }
    public class ValidateRowsResult
    {
        public List<Failure> Failures { get; set; }
        public List<RowDetail> ValidRows { get; set; }
    }
}
