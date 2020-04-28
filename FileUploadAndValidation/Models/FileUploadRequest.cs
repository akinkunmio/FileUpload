﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class FileUploadRequest
    {
        public string AuthToken { get; set; }

        public string ContentType { get; set; }

        public string FileName { get; set; }

        public long FileSize { get; set; }

        public string ItemType { get; set; }

        public string FileExtension { get; set; }

        public string RawFileLocation { get; set; }

        public long? UserId { get; set; }

        public string ProductCode { get; set; }
        public IFormFile FileRef { get; private set; }

        public static FileUploadRequest FromRequest(HttpRequest request)
        {
            var file = request.Form.Files.First();
            var userId = request.Form["id"].ToString();
            var productCode = request.Form["productCode"].ToString();

            return new FileUploadRequest
            {
                FileRef = file,
                AuthToken = request.Headers["Authorization"],
               // ContentType = request.Query["itemType"],
                FileName = file.FileName.Split('.')[0],
                FileSize = file.Length,
                FileExtension = Path.GetExtension(file.FileName)
                                    .Replace(".", string.Empty)
                                    .ToLower(),
                ItemType = request.Query["itemType"],
                UserId = long.Parse(userId),
                ProductCode = productCode
            };
        }
    }
}
}
