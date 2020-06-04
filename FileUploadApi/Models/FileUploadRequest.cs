//using FileUploadAndValidation.Models;
//using FilleUploadCore.Exceptions;
//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace FileUploadApi.Models
//{
//    public class FileUploadRequest : IFileUploadRequest
//    {
//        public string AuthToken { get; set; }

//        public string ContentType { get; set; }

//        public string FileName { get; set; }

//        public long FileSize { get; set; }

//        public string ItemType { get; set; }

//        public string FileExtension { get; set; }

//        public string RawFileLocation { get; set; }

//        public long? UserId { get; set; }

//        public string ProductCode { get; set; }
//        public IFormFile FileRef { get; private set; }

//        public FileUploadRequest(HttpRequest request)
//        {
//            var file = request.Form.Files.FirstOrDefault() ?? throw new AppException("No file uploaded");
//            if (!long.TryParse(request.Form["id"].ToString(), out long userId))
//                throw new AppException($"Invalid value user id:'{userId}' passed!.");

//            var productCode = request.Form["productCode"].ToString();
     
//            FileRef = file;
//            AuthToken = request.Headers["Authorization"];
//            ItemType = request.Query["itemType"];
//            UserId = userId;
//            ProductCode = productCode;

//            ContentType = request.Query["itemType"];
//            FileName = file.FileName.Split('.')[0];
//            FileSize = file.Length;
//            FileExtension = Path.GetExtension(file.FileName)
//                                .Replace(".", string.Empty)
//                                .ToLower();
//        }
//    }
//}