using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class ValidateFileNasModel
    {
        public string Authority { get; set; }

        public string TaxType { get; set; }

        public IEnumerable<dynamic> Taxes { get; set; }
    }
}
