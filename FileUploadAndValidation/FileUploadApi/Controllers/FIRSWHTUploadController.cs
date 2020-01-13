using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation.DTOs;
using FileUploadAndValidation.Models;
using FileUploadApi.Models;
using FileUploadApi.Services;
using FilleUploadCore.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileUploadApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FIRSWHTUploadController : ControllerBase
    {
        private readonly IFileUploadService<FirsWhtUploadResult> _firsFileService;
        public FIRSWHTUploadController(IFileUploadService<FirsWhtUploadResult> firsFileService)
        {
            _firsFileService = firsFileService;
        }
       
        [HttpPost("multipartsfileupload")]
        public async Task<IActionResult> PostMultipartsFileUploadAsync()
        {
            var fileUploadResults = new List<FileUploadResult>();

            foreach (var file in Request.Form.Files)
            {
                if (file.ContentType.Equals("text") 
                    || file.FileName.Split('.').Last().Equals("txt", StringComparison.InvariantCultureIgnoreCase))
                {
                    using (var fileStream = new MemoryStream())
                    {
                        file.CopyTo(fileStream);
                        fileUploadResults.Add(await _firsFileService.ProcessTxtCsvFile(fileStream.ToArray()));
                    }
                }
                if (file.ContentType.Equals("application/vnd.ms-excel") 
                    || file.FileName.Split('.').Last().Equals("xls", StringComparison.InvariantCultureIgnoreCase))
                {
                    using (var fileStream = new MemoryStream())
                    {
                        file.CopyTo(fileStream);
                        fileUploadResults.Add(await _firsFileService.ProcessXlsFile(fileStream.ToArray()));
                    }
                }
                if (file.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.InvariantCultureIgnoreCase)
                    || file.FileName.Split('.').Last().Equals("xlsx", StringComparison.InvariantCultureIgnoreCase))
                {
                    using (var fileStream = new MemoryStream())
                    {
                        file.CopyTo(fileStream);
                        fileUploadResults.Add(await _firsFileService.ProcessXlsxFile(fileStream.ToArray()));
                    }
                }

            }

            return Ok(fileUploadResults);
        }

        [HttpGet("ping")]
        public IActionResult Get()
        {
            return Ok(new string[] { "hello world", "this is upload service"});
        }

        [HttpPost("_uploadbase64string")]
        public async Task<IActionResult> UploadBase64Async([FromBody] FileUploadModel model)
        {
            ArgumentGuard.NotNull(model, nameof(model));
            ArgumentGuard.NotNullOrWhiteSpace(model.Base64EncodedString, nameof(model.Base64EncodedString));
            ArgumentGuard.NotNullOrWhiteSpace(model.FileType, nameof(model.FileType));

            var result = new FirsWhtUploadResult();

            if (model.FileType.Equals("txt", StringComparison.InvariantCultureIgnoreCase) 
                || model.FileType.Equals("csv", StringComparison.InvariantCultureIgnoreCase))
                result = await _firsFileService.ProcessTxtCsvFile(Convert.FromBase64String(model.Base64EncodedString));

            if (model.FileType.Equals("xls", StringComparison.InvariantCultureIgnoreCase))
                result = await _firsFileService.ProcessXlsFile(Convert.FromBase64String(model.Base64EncodedString));

            if (model.FileType.Equals("xlsx", StringComparison.InvariantCultureIgnoreCase))
                result = await _firsFileService.ProcessXlsxFile(Convert.FromBase64String(model.Base64EncodedString));

            return Ok(result);
        }




    }
}
