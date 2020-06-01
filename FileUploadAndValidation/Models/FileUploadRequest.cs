using FileUploadAndValidation.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class FileUploadRequest
    {
        public bool HasHeaderRow { get; set; }

        public string AuthToken { get; set; }

        public string ContentType { get; set; }

        public string FileName { get; set; }

        public long FileSize { get; set; }

        public string ItemType { get; set; }

        public string FileExtension { get; set; }

        public string RawFileLocation { get; set; }

        public long? UserId { get; set; }

        public string ProductCode { get; set; }

        public IFormFile FileRef { get; set; }

        public string ProductName { get; set; }

        public string BusinessTin { get; set; }

        //public static FileUploadRequest FromRequestForSingle(HttpRequest request)
        //{
        //    var file = request.Form.Files.First();
        //    var userId = /*request.Form["id"].ToString() ??*/ "255";
        //    var productCode = /*request.Form["productCode"].ToString() ??*/ "AIRTEL";
        //    var productName = /*request.Form["productName"].ToString() ??*/ "AIRTEL";
        //    var businessTin = /*request.Form["businessTin"].ToString() ??*/ "00771252-0001";

        //    return new FileUploadRequest
        //    {
        //        FileRef = file,
        //        AuthToken = request.Headers["Authorization"],
        //        ContentType = request.Path["contentType"],
        //        ItemType = request.Query["itemType"],
        //        FileName = file.FileName.Split('.')[0],
        //        FileSize = file.Length,
        //        FileExtension = Path.GetExtension(file.FileName)
        //                            .Replace(".", string.Empty)
        //                            .ToLower(),
        //        UserId = long.Parse(userId),
        //        ProductCode = productCode,
        //        ProductName = productName,
        //        BusinessTin = businessTin,
        //    };
        //}
    }
}
