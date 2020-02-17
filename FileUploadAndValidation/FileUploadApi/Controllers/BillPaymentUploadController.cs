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
           
            var uploadResult = new UploadResult();

            var file = Request.Form.Files.First();
            try
            {
                var uploadOptions = new UploadOptions
                {
                    ContentType = contentType,
                    FileName = file.FileName.Split('.')[0],
                    FileSize = file.Length,
                    AuthToken = HttpContext.Request.Headers["Authorization"],
                    FileExtension = Path.GetExtension(file.FileName).Replace(".", string.Empty).ToLower(),
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
            catch(Exception ex)
            {
                return BadRequest(new { uploadResult, errorMessage = "Unknown error occured. Please retry!. "+ex.Message });
            }
            
            return Ok(uploadResult);
        }

        [HttpGet("uploadfile/{batchId}/results")]
        public async Task<IActionResult> GetFileUploadResult([FromQuery]string batchId)
        {
            // checks bill payment transactions table for rows that have scheduleId
            //and returns status,transaction details,
            IEnumerable<BillPaymentRowStatus> billPayments;
            try
            {
                billPayments = await _uploadService.GetBillPaymentsStatus(batchId);
            }
            catch(AppException appEx)
            {
                return BadRequest(appEx.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Unknown error occured. Please retry!. " + ex.Message);
            }

            return Ok(billPayments);
        }

        [HttpGet("uploadfile/{batchId}")]
        public async Task<IActionResult> GetUploadFileSummary([FromQuery]string batchId)
        {
            BatchFileSummaryDto batchFileSummaryDto;
            try
            {
                batchFileSummaryDto = await _uploadService.GetFileSummary(batchId);
            }
            catch (AppException appEx)
            {
                return BadRequest(appEx.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Unknown error occured. Please retry!. " + ex.Message);
            }

            return Ok(batchFileSummaryDto);
        }

        [HttpGet("uploadfile/{batchId}/authorize")]
        public async Task<IActionResult> InitiateTransactionsApprovalAsync([FromQuery]string batchId)
        {
            throw new NotImplementedException();
        }

        [HttpGet("ping")]
        public IActionResult Get()
        {
            return Ok(new string[] { "hello world", "this is upload service"});
        }

    }

   
}
