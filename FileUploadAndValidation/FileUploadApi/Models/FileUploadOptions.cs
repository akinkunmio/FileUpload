using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadApi.Models
{
    public class FileUploadOptions
    {
        public bool ValidateAllRows { get; set; }
        public string ContentType { get; set; }
    }
}
