﻿using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadApi.ApiServices;
using FileUploadApi.Models;
using FilleUploadCore.Exceptions;
using FilleUploadCore.UploadManagers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MimeMapping;

namespace FileUploadApi.Controllers
{
    [Route("/qbupload/api/v1/[controller]")]
    [ApiController]
    public class BillPaymentUploadController : ControllerBase
    {
        private readonly IApiUploadService _uploadService;
        private readonly ILogger<BillPaymentUploadController> _logger;
      

        public BillPaymentUploadController(IApiUploadService uploadService, ILogger<BillPaymentUploadController> logger)
        {
            _uploadService = uploadService;
            _logger = logger;
        }
       
        [HttpPost("uploadfile/{itemType}")]
        public async Task<IActionResult> PostBulkBillPaymentAsync(string itemType)
        {
            var uploadResult = new UploadResult();

            var file = Request.Form.Files.First();
            var userId = Request.Form["userId"].ToString();

            try
            {
                var uploadOptions = new UploadOptions
                {
                    AuthToken = HttpContext.Request.Headers["Authorization"],
                    ContentType = GenericConstants.BillPayment,
                    FileName = file.FileName.Split('.')[0],
                    FileSize = file.Length,
                    FileExtension = Path.GetExtension(file.FileName).Replace(".", string.Empty).ToLower(),
                    ItemType = itemType,
                    UserId = Convert.ToInt64(userId)
                };

                using (var contentStream = file.OpenReadStream())
                {
                    uploadResult = await _uploadService.UploadFileAsync(uploadOptions, contentStream);
                }
            }
            catch(AppException ex)
            {
                _logger.LogError("Could not successfully conclude the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message })
                {
                    StatusCode = ex.StatusCode,
                    Value = uploadResult
                };
                uploadResult.ErrorMessage = ex.Message;

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError("An Unexpected Error occured during Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                return BadRequest(new { uploadResult, errorMessage = "Unknown error occured. Please retry!. |"+ex.Message });
            }
            
            return Ok(uploadResult);
        }

        [HttpGet("{batchId}/status")]
        public async Task<IActionResult> GetFileUploadResult(string batchId, [FromQuery] PaginationQuery pagination)
        {
            var paginationFilter = 
                new PaginationFilter 
                { 
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize 
                };

            var response = new PagedResponse<BillPaymentRowStatus>()
            {
                PageSize = paginationFilter.PageSize,
                PageNumber = paginationFilter.PageNumber,
            };

            try
            {
                var result =  await _uploadService.GetBillPaymentsStatus(batchId, paginationFilter);
                response.Data = result.RowStatuses;
                response.TotalCount = result.TotalRowsCount;
            }
            catch(AppException ex)
            {
                _logger.LogError("Could not get the statuses of rows with BatchId {batchId} : {ex.Message} | {ex.StackTrace}", batchId, ex.Message, ex.StackTrace);
                response.Error = ex.Message;
                var result = new ObjectResult(new { ex.Message })
                {
                    StatusCode = ex.StatusCode,

                    Value = response
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                return BadRequest("Unknown error occured. Please retry!. |" + ex.Message);
            }

            return Ok(new { batchId, response });
        }

        [HttpGet("{batchId}/summary")]
        public async Task<IActionResult> GetUploadFileSummary(string batchId)
        {
            var response = new ResponseModel();
            
            try
            {
                response.Data = await _uploadService.GetFileSummary(batchId);
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured during the Upload File Process:{ex.Value} | {ex.Message} | {ex.StackTrace}", ex.Value, ex.Message, ex.StackTrace);

                response.Error = ex.Message;
                var result = new ObjectResult(response)
                {
                    StatusCode = ex.StatusCode
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
               
                response.Error = "Unknown error occured. Please retry!.";
                var result = new ObjectResult(response);

                result.StatusCode = (int)HttpStatusCode.BadRequest;
                return result;
            }

            return Ok(response);
        }

        [HttpPost("{batchId}/authorize")]
        public async Task<IActionResult> InitiateTransactionsApprovalAsync(string batchId, [FromBody] InitiatePaymentRequest request)
        {
            var response = new ResponseModel();
            try
            {
                if (HttpContext.Request.Headers["Authorization"].ToString() == null)
                    throw new AppException("'Auth Token' cannot be null or empty", (int)HttpStatusCode.Unauthorized);

                var initiatePaymentOptions = new InitiatePaymentOptions()
                {
                    AuthToken = HttpContext.Request.Headers["Authorization"],
                    ApprovalConfigId = request.ApprovalConfigId,
                    BusinessId = request.BusinessId,
                    UserId = request.UserId,
                    UserName = request.UserName
                };


                var data = await _uploadService.PaymentInitiationConfirmed(batchId, initiatePaymentOptions);
                response.Data = data;

            }
            catch (AppException ex)
            {
               // _logger.LogError("Could not get the required Initiate Payment for Batch with Id {batchid} : {ex.Message} | {ex.StackTrace}", batchId, ex.Message, ex.StackTrace);

                response.Error = ex.Message;
                var result = new ObjectResult(new { ex.Message })
                {
                    StatusCode = ex.StatusCode,
                    Value = response
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during initiate transactions approval: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                
                response.Error = "Unknown error occured. Please retry!.";
                var result = new ObjectResult(response)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return result;
            }

            return Ok(response);
        }

        [HttpGet("template/{extension}/download")]
        public async Task<IActionResult> GetTemplate(string extension)
        {
            try
            {
                var outputStream = new MemoryStream();

                var filePath = await _uploadService.GetFileTemplateContentAsync(extension, outputStream);

                var contentType = MimeUtility.GetMimeMapping(filePath);

                outputStream.Seek(0, SeekOrigin.Begin);

                return File(outputStream, contentType, GenericConstants.BillPaymentTemplate+"."+extension);
            }
            catch(AppException ex)
            {
                _logger.LogError("An Error occured during the Template Download File Process:{ex.Value} | {ex.Message} | {ex.StackTrace}", ex.Value, ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError("An Error occured during the Template Download File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult("Unknown error occured. Please retry!.")
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return result;
            }
        }

        [HttpPost("user/uploads")]
        public async Task<IActionResult> GetUserUploadedFilesSummary([FromBody] string userId)
        {
            var response = new ResponseModel();

            try
            {
                response.Data = await _uploadService.GetUserFilesSummary(userId);
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured during the Upload File Process:{ex.Value} | {ex.Message} | {ex.StackTrace}", ex.Value, ex.Message, ex.StackTrace);

                response.Error = ex.Message;
                var result = new ObjectResult(response)
                {
                    StatusCode = ex.StatusCode
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                response.Error = "Unknown error occured. Please retry!.";
                var result = new ObjectResult(response);

                result.StatusCode = (int)HttpStatusCode.BadRequest;
                return result;
            }

            return Ok(response);
        }
    }
}
