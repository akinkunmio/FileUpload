using FileUploadAndValidation.Helpers;
using FilleUploadCore.Exceptions;
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
        public UserContext User {get;set;}

        public string ProductCode { get; set; }

        public IFormFile FileRef { get; set; }

        public string ProductName { get; set; }

        public string BusinessTin { get; set; }

        public static FileUploadRequest FromRequestForSingle(HttpRequest request)
        {
            var file = request.Form.Files.FirstOrDefault();
            if(file == null) throw new AppException("No file uploaded", "No file uploaded");

            var userId = /*request.Form["id"].ToString() ??*/ "255";
            var productCode = /*request.Form["productCode"].ToString() ??*/ "AIRTEL";
            var productName = /*request.Form["productName"].ToString() ??*/ "AIRTEL";
            var businessTin = /*request.Form["businessTin"].ToString() ??*/ "00771252-0001";

            return new FileUploadRequest
            {
                FileRef = file,
                AuthToken = request.Headers["Authorization"] + "1234",
                ContentType = request.Query["contentType"],
                ItemType = request.Query["itemType"],
                FileName = file.FileName.Split('.')[0],
                FileSize = file.Length,
                FileExtension = Path.GetExtension(file.FileName)
                                    .Replace(".", string.Empty)
                                    .ToLower(),
                UserId = long.Parse(userId),
                ProductCode = productCode,
                ProductName = productName,
                BusinessTin = businessTin,
            };
        }

        public static FileUploadRequest FromRequestForFirsMultitax(HttpRequest request, string authority)
        {
            var userId = request.Form["id"].ToString();

            bool success = long.TryParse(userId, out long number);

            if (!success)
            {
                throw new AppException($"Invalid value '{userId}' passed for 'id'!.", 400);
            }

            if (string.IsNullOrWhiteSpace(request.Form["HasHeaderRow"].ToString()))
                throw new AppException("Value must be passed for 'HasHeaderRow'.");

            var file = request.Form.Files.FirstOrDefault();

            if (file == default)
                throw new AppException("Please upload a file.");

            return new FileUploadRequest
            {
                ItemType = GenericConstants.MultiTax,
                ContentType = authority,
                AuthToken = request.Headers["Authorization"].ToString(),
                FileRef = file,
                FileName = file.FileName
                                    .Split('.')[0],
                FileExtension = Path.GetExtension(file.FileName)
                                .Replace(".", string.Empty)
                                .ToLower(),
                UserId = long.Parse(userId),
                ProductCode = request.Form["productCode"].ToString(),
                ProductName = request.Form["productName"].ToString(),
                FileSize = file.Length,
                HasHeaderRow = request.Form["HasHeaderRow"].ToString().ToBool() 
            };

        }


        public static FileUploadRequest FromRequestForFCTIRS(HttpRequest request)
        {
            var file = request.Form.Files.FirstOrDefault();
            if(file == null) throw new AppException("No file uploaded", "No file uploaded");
            
            return new FileUploadRequest 
            {
                    ItemType = GenericConstants.FctIrs,
                    ContentType = GenericConstants.FctIrs,
                    AuthToken = request.Headers["Authorization"].ToString(),
                    FileRef = file,
                    FileName = file.FileName.Split('.')[0],
                    FileExtension = Path.GetExtension(file.FileName)
                                    .Replace(".", string.Empty)
                                    .ToLower(),
                    UserId = long.Parse(request.Form["id"]),
                    User = new UserContext {
                        Username = request.Headers["userName"]
                    },
                    FileSize = file.Length
            };
        }

        public static FileUploadRequest FromRequestForLASG(HttpRequest request)
        {
            var file = request.Form.Files.FirstOrDefault();
            if(file == null) throw new AppException("No file uploaded", "No file uploaded");
            
            return new FileUploadRequest 
            {
                    ItemType = GenericConstants.FctIrs,
                    ContentType = GenericConstants.FctIrs,
                    AuthToken = request.Headers["Authorization"].ToString(),
                    FileRef = file,
                    FileName = file.FileName.Split('.')[0],
                    FileExtension = Path.GetExtension(file.FileName)
                                    .Replace(".", string.Empty)
                                    .ToLower(),
                    UserId = long.Parse(request.Form["id"]),
                    User = new UserContext {
                        Username = request.Headers["userName"]
                    },
                    FileSize = file.Length
            };
        }

        public static FileUploadRequest FromRequestForMultiple(HttpRequest request) 
        {
            var userId = /*request.Form["id"].ToString() ??*/ "255";
            
            return new FileUploadRequest 
            {
                    ItemType = GenericConstants.Lasg,
                    ContentType = GenericConstants.Lasg,
                    AuthToken = request.Headers["Authorization"].ToString(),
                    FileRef = request.Form.Files.First(),
                    FileName = request.Form.Files.First().FileName.Split('.')[0],
                    FileExtension = Path.GetExtension(request.Form.Files.First().FileName)
                                    .Replace(".", string.Empty)
                                    .ToLower(),
                    UserId = long.Parse(userId),
                    FileSize = request.Form.Files.First().Length
            };
        }
    }

    public class UserContext
    {
        public string Username { get; set;}
        public string InstitutionCode {get; set; }
        public string InstitutionType { get;set; }
        public string RoleName { get; set; }
    }
}
