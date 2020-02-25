using System;
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
using Microsoft.Extensions.Logging;

namespace FileUploadApi.Controllers
{
    [Route("api/[controller]")]
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
                };

                using (var contentStream = file.OpenReadStream())
                {
                    uploadResult = await _uploadService.UploadFileAsync(uploadOptions, contentStream);
                }
            }
            catch(AppException ex)
            {
                _logger.LogError("Could not successfully conclude the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message });
                uploadResult.ErrorMessage = ex.Message;

                result.StatusCode = ex.StatusCode; result.Value = uploadResult;

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError("An Unexpected Error occured during Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                return BadRequest(new { uploadResult, errorMessage = "Unknown error occured. Please retry!. |"+ex.Message });
            }
            
            return Ok(uploadResult);
        }

        [HttpGet("uploadfile/{batchId}/results")]
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
                response.Data = await _uploadService.GetBillPaymentsStatus(batchId, paginationFilter);
            }
            catch(AppException ex)
            {
                _logger.LogError("Could not get the statuses of rows with BatchId {batchId} : {ex.Message} | {ex.StackTrace}", batchId, ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message });

                result.StatusCode = ex.StatusCode; 
                result.Value = ex.Value;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                return BadRequest("Unknown error occured. Please retry!. |" + ex.Message);
            }

            return Ok(new { batchId, response });
        }

        [HttpGet("uploadfile/{batchId}")]
        public async Task<IActionResult> GetUploadFileSummary(string batchId)
        {
            BatchFileSummaryDto batchFileSummaryDto;
            try
            {
                batchFileSummaryDto = await _uploadService.GetFileSummary(batchId);
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured during the Upload File Process:{ex.Value} | {ex.Message} | {ex.StackTrace}", ex.Value, ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message });

                result.StatusCode = ex.StatusCode; 
                result.Value = ex.Value;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

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
                _logger.LogError("Could not get the required Initiate Payment for Batch with Id {batchid} : {ex.Message} | {ex.StackTrace}", batchId, ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message });

                result.StatusCode = ex.StatusCode; 
                result.Value = ex.Value;

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError("An Error occured during initiate transactions approval: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                return BadRequest(new { errorMessage = "Unknown error occured. Please retry!.  |" + ex.Message });
            }

            return Ok(new { Status = "PaymentInitiated" });
        }

    }
}
