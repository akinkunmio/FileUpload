using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.Models;
using FileUploadApi.ApiServices;
using FileUploadApi.Models;
using FileUploadApi.Services;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileUploadApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FIRSWHTUploadController : ControllerBase
    {
        private readonly IApiUploadService _uploadService;
      

        public FIRSWHTUploadController(IApiUploadService uploadService)
        {
            _uploadService = uploadService;
        }
       
        [HttpPost("multipartsfileupload")]
        public async Task<IActionResult> PostMultipartsFileUploadAsync()
        {
            var fileUploadResult = new UploadResult();
            var file = Request.Form.Files.First();

            if ((file.ContentType.Equals("text/plain", StringComparison.InvariantCultureIgnoreCase)
                || file.FileName.Split('.').Last().Equals("txt", StringComparison.InvariantCultureIgnoreCase)) 
                || (file.ContentType.Equals("application/vnd.ms-excel", StringComparison.InvariantCultureIgnoreCase)
                && file.FileName.Split('.').Last().Equals("csv", StringComparison.InvariantCultureIgnoreCase)))
            {
                using (var fileStream = new MemoryStream())
                {
                    file.CopyTo(fileStream);
                    fileUploadResult = await _uploadService.UploadFileAsync(FileTypes.TXT, fileStream.ToArray());
                }
            }
            if (file.ContentType.Equals("application/vnd.ms-excel", StringComparison.InvariantCultureIgnoreCase)
                && file.FileName.Split('.').Last().Equals("xls", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var fileStream = new MemoryStream())
                {
                    file.CopyTo(fileStream);
                    fileUploadResult = await _uploadService.UploadFileAsync(FileTypes.XLS,fileStream.ToArray());
                }
            }
            if (file.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.InvariantCultureIgnoreCase)
                && file.FileName.Split('.').Last().Equals("xlsx", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var fileStream = new MemoryStream())
                {
                    file.CopyTo(fileStream);
                    fileUploadResult = await _uploadService.UploadFileAsync(FileTypes.XLSX, fileStream.ToArray());
                }
            }

            return Ok(fileUploadResult);
        }

        [HttpGet("ping")]
        public IActionResult Get()
        {
            return Ok(new string[] { "hello world", "this is upload service"});
        }

    }
}
