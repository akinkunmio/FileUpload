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
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using FilleUploadCore.UploadManagers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileUploadApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillPaymentUploadController : ControllerBase
    {
        private readonly IApiUploadService _uploadService;
      

        public BillPaymentUploadController(IApiUploadService uploadService)
        {
            _uploadService = uploadService;
        }
       
        [HttpPost("uploadfile")]
        public async Task<IActionResult> PostBulkBillPaymentAsync()
        {
            var contentType = Request.Form["contentType"].ToString().Trim();
            var fileName = Request.Form["validateHeaders"].ToString().Trim();
            var fileSize = Request.Form["validateHeaders"].ToString().Trim();

            var uploadResult = new UploadResult();

            var file = Request.Form.Files.First();
            try
            {
                var uploadOptions = new UploadOptions
                {
                    ContentType = contentType,
                    FileName = fileName,
                    FileSize = file.Length,
                    AuthToken = HttpContext.Request.Headers["Authorization"],
                    UserName = Request.Headers["userName"]
                };

                using (var contentStream = file.OpenReadStream())
                {
                    uploadResult = await _uploadService.UploadFileAsync(uploadOptions, contentStream);
                }
            }
            catch(AppException appEx)
            {
                return new ObjectResult(new { uploadResult, errorMessage = appEx.Message }) { StatusCode = appEx.StatusCode };
            }
            catch(Exception)
            {
                return BadRequest(new { uploadResult, errorMessage = "Unknown error occured. Please retry!." });
            }
            
            return Ok(uploadResult);
        }

        [HttpGet("uploadfile/{scheduleid}/results")]
        public async Task<IActionResult> GetFileUploadResult([FromQuery]string scheduleId)
        { 
            // checks bill payment transactions table for rows that have scheduleId
            //and returns status,transaction details,
            string userName = Request.Headers["userName"];
            return Ok(await _uploadService.GetBillPaymentsStatus(scheduleId, userName));
        }

        [HttpGet("uploadfile/{scheduleid}")]
        public async Task<IActionResult> GetUploadFileSummary([FromQuery]string scheduleId)
        {
            string userName = Request.Headers["userName"];
            return Ok(await _uploadService.GetBatchFileSummary(scheduleId, userName));
        }

        //[HttpGet("uploadfile/{scheduleid}/authorize")]
        //public async Task<IActionResult> InitiateTransactionsApprovalAsync([FromQuery]string scheduleId)
        //{
        //}

        [HttpGet("ping")]
        public IActionResult Get()
        {
            return Ok(new string[] { "hello world", "this is upload service"});
        }

    }
}
