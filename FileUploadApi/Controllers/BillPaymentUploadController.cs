﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.Helpers;
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
       
        [HttpPost("uploadfile/{itemType}")]
        public async Task<IActionResult> PostBulkBillPaymentAsync(string itemType)
        {
            var uploadResult = new UploadResult();

            var file = Request.Form.Files.First();
            
            try
            {
                if (string.IsNullOrEmpty(itemType))
                    throw new AppException("Item type cannot be empty");

                if (!itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()) 
                    && !itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower()))
                    throw new AppException("Invalid Item Type specified");
           
                var uploadOptions = new UploadOptions
                {
                    ContentType = "BillPayment",
                    FileName = file.FileName.Split('.')[0],
                    FileSize = file.Length,
                    FileExtension = Path.GetExtension(file.FileName).Replace(".", string.Empty).ToLower(),
                    ItemType = itemType,
                };

                using (var contentStream = file.OpenReadStream())
                {
                    uploadResult = await _uploadService.UploadFileAsync(uploadOptions, contentStream);
                }
            }
            catch(AppException appEx)
            {
                return new ObjectResult(new { appEx.Message }) { StatusCode = appEx.StatusCode, Value = appEx.Value };
            }
            catch(Exception ex)
            {
                return BadRequest(new { uploadResult, errorMessage = "Unknown error occured. Please retry!.  |"+ex.Message });
            }
            
            return Ok(uploadResult);
        }

        [HttpGet("uploadfile/{batchId}/results")]
        public async Task<IActionResult> GetFileUploadResult(string batchId)
        {
            IEnumerable<BillPaymentRowStatus> billPayments;
            try
            {
                billPayments = await _uploadService.GetBillPaymentsStatus(batchId);
            }
            catch(AppException appEx)
            {
                return new ObjectResult(new { errorMessage = appEx.Message }) { StatusCode = appEx.StatusCode };
            }
            catch (Exception ex)
            {
                return BadRequest("Unknown error occured. Please retry!. " + ex.Message);
            }

            return Ok(new { batchId, billPayments });
        }

        [HttpGet("uploadfile/{batchId}")]
        public async Task<IActionResult> GetUploadFileSummary(string batchId)
        {
            BatchFileSummaryDto batchFileSummaryDto;
            try
            {
                batchFileSummaryDto = await _uploadService.GetFileSummary(batchId);
            }
            catch (AppException appEx)
            {
                return new ObjectResult(new { errorMessage = appEx.Message }) { StatusCode = appEx.StatusCode };
            }
            catch (Exception ex)
            {
                return BadRequest("Unknown error occured. Please retry!. " + ex.Message);
            }

            return Ok(batchFileSummaryDto);
        }

        [HttpPost("uploadfile/{batchId}/authorize")]
        public async Task<IActionResult> InitiateTransactionsApprovalAsync(string batchId, [FromBody] InitiatePaymentRequest request)
        {
            try
            {
                ArgumentGuard.NotNullOrWhiteSpace(request.UserName, nameof(request.UserName));
                ArgumentGuard.NotDefault(request.UserId, nameof(request.UserId));
                ArgumentGuard.NotDefault(request.BusinessId, nameof(request.BusinessId));
                ArgumentGuard.NotDefault(request.ApprovalConfigId, nameof(request.ApprovalConfigId));

                var initiatePaymentOptions = new InitiatePaymentOptions()
                {
                    AuthToken = HttpContext.Request.Headers["Authorization"],
                    ApprovalConfigId = request.ApprovalConfigId,
                    BusinessId = request.BusinessId,
                    UserId = request.UserId,
                    UserName = request.UserName
                };
             
               await _uploadService.PaymentInitiationConfirmed(batchId, initiatePaymentOptions);
            }
            catch (AppException ex)
            {
                return new ObjectResult(new { ex.Message }) { StatusCode = ex.StatusCode, Value = ex.Value };
            }
            catch(Exception ex)
            {
                return BadRequest(new { errorMessage = "Unknown error occured. Please retry!.  |" + ex.Message });
            }

            return Ok();
        }

        [HttpGet("ping")]
        public IActionResult Get()
        {
            return Ok(new string[] { "hello world", "this is upload service"});
        }

    }

   
}
