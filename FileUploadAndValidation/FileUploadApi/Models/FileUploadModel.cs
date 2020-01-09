using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadApi.Models
{
    public class FileUploadModel
    {
        public string Base64EncodedString { get; set; }
        public string FileType { get; set; }
    }
}
