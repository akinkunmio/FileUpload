using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadApi.ApiServices;
using FileUploadApi.Models;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FileUploadApi.Controllers
{
    [Route("/qbupload/api/v1/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IMultiTaxProcessor _multiTaxProcessor;
        private readonly IGenericUploadService _genericUploadService;
        private readonly IBatchProcessor _batchProcessor;
        private readonly ILogger<UploadController> _logger;
        private readonly IEnumerable<IFileReader> _fileReaders;


        public UploadController(IBatchProcessor batchProcessor,
            ILogger<UploadController> logger,
            IGenericUploadService genericUploadService,
            IMultiTaxProcessor multiTaxProcessor)
        {
            _logger = logger;
            _batchProcessor = batchProcessor;
            _genericUploadService = genericUploadService;
            _multiTaxProcessor = multiTaxProcessor;
        }

        [HttpPost("uploadfile/{contentType}/{itemType}")]
        public async Task<IActionResult> PostBulkUploadPaymentAsync(string contentType, string itemType)
        {
            ResponseResult response;

            var userId = Request.Form["id"].ToString() /*"255"*/;

            try
            {
                ValidateUserId(userId);

                var request = new FileUploadRequest
                {
                    ItemType = itemType,
                    ContentType = contentType,
                    AuthToken = Request.Headers["Authorization"].ToString(),
                    FileRef = Request.Form.Files.First(),
                    FileName = Request.Form.Files
                                        .First().FileName
                                        .Split('.')[0],
                    FileExtension = Path.GetExtension(Request.Form.Files.First().FileName)
                                    .Replace(".", string.Empty)
                                    .ToLower(),
                    UserId = long.Parse(userId),
                    ProductCode = Request.Form["productCode"].ToString() ??"AIRTEL",
                    ProductName = Request.Form["productName"].ToString() ?? "AIRTEL",
                    BusinessTin = Request.Form["businessTin"].ToString() ?? "00771252-0001",
                    FileSize = Request.Form.Files.First().Length,
                    HasHeaderRow = Request.Form["HasHeaderRow"].ToString().ToBool()
                };

                response = await _batchProcessor.UploadFileAsync(request);
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Unexpected Error occured ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                return BadRequest(new { errorMessage = "An error occured. Please retry!." });
            }

            return Ok(response);
        }
       

        [HttpPost("multitax/{authority}")]
        public async Task<IActionResult> PostMultiTaxPaymentUploadAsync(string authority)
        {
            ResponseResult response;

            var userId = Request.Form["id"].ToString() /*"255"*/;

            try
            {
                ValidateUserId(userId);

                if (string.IsNullOrWhiteSpace(Request.Form["HasHeaderRow"].ToString()))
                    throw new AppException("Value must be passed for 'HasHeaderRow'.");

                if (Request.Form.Files.Count() == 0)
                    throw new AppException("Please upload a file."); 

                var request = new FileUploadRequest
                {
                    ItemType = GenericConstants.MultiTax,
                    ContentType = authority,
                    AuthToken = Request.Headers["Authorization"].ToString(),
                    FileRef = Request.Form.Files.First(),
                    FileName = Request.Form.Files
                                        .First().FileName
                                        .Split('.')[0],
                    FileExtension = Path.GetExtension(Request.Form.Files.First().FileName)
                                    .Replace(".", string.Empty)
                                    .ToLower(),
                    UserId = long.Parse(userId),
                    ProductCode = Request.Form["productCode"].ToString(),
                    ProductName = Request.Form["productName"].ToString(),
                    FileSize = Request.Form.Files.First().Length,
                    HasHeaderRow = Request.Form["HasHeaderRow"].ToString().ToBool() /*true*/
                };

                response = await _multiTaxProcessor.UploadFileAsync(request);
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { errorMessage = ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Unexpected Error occured ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                return BadRequest(new { errorMessage = "An error occured. Please retry!." });
            }

            return Ok(response);
        }


        [HttpGet("{batchId}/status")]
        public async Task<IActionResult> GetFileUploadResult(string batchId, [FromQuery] PaginationQuery pagination)
        {

            var response = new PagedResponse<dynamic>()
            {
                PageSize = pagination.PageSize,
                PageNumber = pagination.PageNumber,
            };
                
            try
            {
                var status = (Enum.IsDefined(typeof(StatusEnum), pagination.Status))
                ? (StatusEnum)pagination.Status
                : throw new AppException("The field 'Status' must have a value between 0 and 2.");

                string auth = Request.Headers["Authorization"].ToString();

                var paginationFilter =
                   new PaginationFilter(pagination.PageSize,
                   pagination.PageNumber,
                   status,
                   pagination.TaxType
                   );

                var result = await _genericUploadService.GetPaymentsStatus(batchId, paginationFilter, auth);

                response.Data = result.Data;
                response.TotalCount = result.TotalRowsCount;
                response.ValidAmountTotal = result.TotalAmountSum;
                response.ProductName = result.ProductName;
                response.ProductCode = result.ProductCode;
                response.FileName = result.FileName;
                response.BatchId = batchId;
                response.ItemType = result.ItemType;
                response.ContentType = result.ContentType;
                response.ValidCount = result.ValidRowCount;
                response.InvalidCount = result.InvalidCount;
                response.Status = ((StatusEnum)pagination.Status).ToString();
                response.IsValidated = result.IsValidated.ToNonNullBool();

            }
            catch (AppException ex)
            {
                _logger.LogError("Could not get the statuses of rows with BatchId {batchId} : {ex.Message} | {ex.StackTrace}", batchId, ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { errorMessage = ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                return BadRequest("An error occured. Please retry!");

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
                    throw new AppException("'Auth Token' cannot be null or empty!.", (int)HttpStatusCode.Unauthorized);

                var initiatePaymentOptions = new InitiatePaymentOptions()
                {
                    AuthToken = HttpContext.Request.Headers["Authorization"],
                    ApprovalConfigId = request.ApprovalConfigId,
                    BusinessId = request.BusinessId,
                    UserId = request.UserId,
                    UserName = request.UserName,
                    BusinessTin = request.BusinessTin,
                    TaxTypeId = request.TaxTypeId,
                    TaxTypeName = request.TaxTypeName,
                    ProductId = request.ProductId,
                    ProductCode = request.ProductCode,
                    CurrencyCode = request.CurrencyCode
                };

                response.Data = await _genericUploadService.PaymentInitiationConfirmed(batchId, initiatePaymentOptions);
            }
            catch (AppException ex)
            {
                // _logger.LogError("Could not get the required Initiate Payment for Batch with Id {batchid} : {ex.Message} | {ex.StackTrace}", batchId, ex.Message, ex.StackTrace);

                //response.Message = ex.Message;

                var result = new ObjectResult(new { errorMessage = ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during initiate transactions approval: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { errorMessage = "An error occured.Please retry!." })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return result;
            }

            return Ok(response);
        }

        [HttpGet("template/{contentType}/{itemType}/download")]
        public async Task<IActionResult> GetTemplate(string contentType, string itemType)
        {
            try
            {
                var outputStream = new MemoryStream();

                var fileName = await _genericUploadService.GetFileTemplateContentAsync(contentType, itemType, outputStream);

                var extension = MimeUtility.GetMimeMapping(fileName);

                outputStream.Seek(0, SeekOrigin.Begin);

                return File(outputStream, extension, fileName);
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured during the Template Download File Process:{ex.Value} | {ex.Message} | {ex.StackTrace}", ex.Value, ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { errorMessage = ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during the Template Download File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { errorMessage = "An error occured. Please retry!." })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return result;
            }
        }

        private void ValidateUserId(string id)
        {
            bool success = long.TryParse(id, out long number);

            if (!success)
            {
                throw new AppException($"Invalid value '{id}' passed for 'id'!.", 400);
            }
        }


        [HttpPost("user/uploads")]
        public async Task<IActionResult> GetUserUploadedFilesSummary([FromBody] string userId, [FromQuery] SummaryPaginationQuery pagination)
        {
            
            var response = new SummaryPagedResponse<BatchFileSummaryDto>();

            try
            {
                var status = (Enum.IsDefined(typeof(SummaryStatusEnum), pagination.Status))
                               ? (SummaryStatusEnum)pagination.Status
                               : throw new AppException("The field 'Status' must have a value between 0 and 3.");

                var paginationFilter = new SummaryPaginationFilter
                {
                    PageSize = pagination.PageSize,
                    PageNumber = pagination.PageNumber,
                    Status = (SummaryStatusEnum)pagination.Status,
                    ProductCode = pagination.ProductCode,
                    ProductName = pagination.ProductName
                };

                ValidateUserId(userId);

                var result = await _genericUploadService.GetUserFilesSummary(userId, paginationFilter);
                response.Data = result.Data;
                response.TotalCount = result.TotalRowsCount;
                response.PageSize = paginationFilter.PageSize;
                response.PageNumber = paginationFilter.PageNumber;
                response.ProductCode = paginationFilter.ProductCode;
                response.ProductName = paginationFilter.ProductName;
                response.Status = (paginationFilter.Status).ToString();
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured  {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { errorMessage = ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                return BadRequest(new { errorMessage = "An error occured. Please retry!." });
            }

            return Ok(response);
        }

        [HttpGet("download/{batchId}/statusresult")]
        public async Task<IActionResult> ValidationResultFile(string batchId)
        {
            try
            {
                var outputStream = new MemoryStream();

                var resultModel = await _genericUploadService.GetFileValidationResultAsync(batchId, outputStream);

                var contentType = MimeUtility.GetMimeMapping(resultModel.NasValidationFileName);

                outputStream.Seek(0, SeekOrigin.Begin);

                return File(outputStream, contentType, resultModel.RawFileName + '_' + GenericConstants.ValidationResultFile);
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { errorMessage = ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                return BadRequest(new { errorMessage = "An error occured.Please, retry!." });
            }
        }
    }
}
