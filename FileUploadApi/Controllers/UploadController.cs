using FileUploadAndValidation.Helpers;
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
using FilleUploadCore.FileReaders;
using System.Collections.Generic;
using AutoMapper;

namespace FileUploadApi.Controllers
{
    [Route("/qbupload/api/v1/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IBatchProcessor _batchProcessor;
        private readonly ILogger<UploadController> _logger;


        public UploadController(IBatchProcessor batchProcessor, 
            ILogger<UploadController> logger)
        {
            _logger = logger;
            _batchProcessor = batchProcessor;
        }

        [HttpPost("uploadfile/{contentType}/{itemType}")]
        public async Task<IActionResult> PostBulkUploadPaymentAsync(string contentType, string itemType)
        {
            var uploadResult = new UploadResult();

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
                    FileName = Request.Form.Files.First().FileName.Split('.')[0],
                    FileExtension = Path.GetExtension(Request.Form.Files.First().FileName)
                                    .Replace(".", string.Empty)
                                    .ToLower(),
                    UserId = long.Parse(userId),
                    ProductCode = Request.Form["productCode"].ToString() /*?? "AIRTEL"*/,
                    ProductName = Request.Form["productName"].ToString() /*?? "AIRTEL"*/,
                    BusinessTin = Request.Form["businessTin"].ToString() /*?? "00771252-0001"*/,
                    FileSize = Request.Form.Files.First().Length,
                };

                uploadResult = await _batchProcessor.UploadFileAsync(request);
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

                return BadRequest(new { errorMessage = "Unknown error occured. Please retry!." });
            }

            return Ok(new ResponseResult
            {
                BatchId = uploadResult.BatchId,
                ValidRows = uploadResult.ValidRows.Select(row => RowMarshaller(row, contentType, itemType)).ToList(),
                Failures = uploadResult.Failures.Select(a => new ResponseResult.FailedValidation
                {
                    ColumnValidationErrors = a.ColumnValidationErrors,
                    Row = RowMarshaller(a.Row, contentType, itemType)
                }).ToList(),
                ErrorMessage = uploadResult.ErrorMessage,
                FileName = uploadResult.FileName,
                ProductCode = uploadResult.ProductCode,
                ProductName = uploadResult.ProductName,
                RowsCount = uploadResult.RowsCount
            });
        }

        private dynamic RowMarshaller(RowDetail r, string contentType, string itemType)
        {
            dynamic result = default;
            
                if (contentType.ToLower().Equals(GenericConstants.Firs)
                    && itemType.ToLower().Equals(GenericConstants.WHT))
                    result = new FirsWhtUntyped 
                    { 
                        Row = r.RowNum,
                        BeneficiaryAddress = r.BeneficiaryAddress,
                        BeneficiaryName = r.BeneficiaryName,
                        BeneficiaryTin = r.BeneficiaryTin,
                        ContractAmount = r.ContractAmount,
                        ContractDate = r.ContractDate,
                        ContractDescription = r.ContractDescription,
                        ContractType = r.ContractType,
                        InvoiceNumber = r.InvoiceNumber,
                        PeriodCovered = r.PeriodCovered,
                        WhtAmount = r.WhtAmount,
                        WhtRate = r.WhtRate
                    };

                if (contentType.ToLower().Equals(GenericConstants.Firs)
                    && itemType.ToLower().Equals(GenericConstants.WVAT))
                    result = new FirsWVatUntyped
                    {
                        Row = r.RowNum,
                        ContractorAddress = r.ContractorAddress,
                        ContractorName = r.ContractorName,
                        ContractorTin = r.ContractorTin,
                        CurrencyExchangeRate = r.CurrencyExchangeRate,
                        CurrencyInvoicedValue = r.CurrencyInvoicedValue,
                        ContractDescription = r.ContractDescription,
                        NatureOfTransaction = r.NatureOfTransaction,
                        InvoiceNumber = r.InvoiceNumber,
                        TaxAccountNumber = r.TaxAccountNumber,
                        TransactionCurrency = r.TransactionCurrency,
                        TransactionDate = r.TransactionDate,
                        TransactionInvoicedValue = r.TransactionInvoicedValue,
                        WvatRate = r.WvatRate,
                        WvatValue = r.WvatValue
                    }; 

                if (contentType.ToLower().Equals(GenericConstants.BillPayment)
                    && (itemType.ToLower().Equals(GenericConstants.BillPaymentId) 
                    || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem)))
                    result = new BillPaymentUntyped
                    {
                        RowNumber = r.RowNum,
                        Amount = r.Amount,
                        CustomerId = r.CustomerId,
                        ItemCode = r.ItemCode,
                        ProductCode = r.ProductCode
                    };
            
            return result;
        }


        [HttpGet("{batchId}/status")]
        public async Task<IActionResult> GetFileUploadResult(string batchId, [FromQuery] PaginationQuery pagination)
        {
            var paginationFilter =
               new PaginationFilter(pagination.PageSize,
               pagination.PageNumber,
               pagination.Status);

            var response = new PagedResponse<dynamic>()
            {
                PageSize = pagination.PageSize,
                PageNumber = pagination.PageNumber,
                Status = pagination.Status.ToString()
            };

            try
            {
                var result = await _batchProcessor.GetBillPaymentsStatus(batchId, paginationFilter);

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
            }
            catch (AppException ex)
            {
                _logger.LogError("Could not get the statuses of rows with BatchId {batchId} : {ex.Message} | {ex.StackTrace}", batchId, ex.Message, ex.StackTrace);
                
                response.Error = ex.Message;
                
                var result = new ObjectResult(new { ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                return BadRequest("Unknown error occured. Please retry!");
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
                    UserName = request.UserName,
                    BusinessTin = request.BusinessTin,
                    TaxTypeId = request.TaxTypeId,
                    TaxTypeName = request.TaxTypeName,
                    ProductId = request.ProductId
                };

                var data = await _batchProcessor.PaymentInitiationConfirmed(batchId, initiatePaymentOptions);
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

        [HttpGet("template/{contentType}/{itemType}/download")]
        public async Task<IActionResult> GetTemplate(string contentType, string itemType)
        {
            try
            {
                var outputStream = new MemoryStream();

                var templateDetail = await _batchProcessor.GetFileTemplateContentAsync(contentType, itemType, outputStream);

                outputStream.Seek(0, SeekOrigin.Begin);

                return File(outputStream, contentType, templateDetail.FileName);
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured during the Template Download File Process:{ex.Value} | {ex.Message} | {ex.StackTrace}", ex.Value, ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured during the Template Download File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult("Unknown error occured. Please retry!.")
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
                throw new AppException($"Invalid value '{id}' passed for 'id'!.");
            }
        }

        [HttpPost("user/uploads")]
        public async Task<IActionResult> GetUserUploadedFilesSummary([FromBody] string userId, [FromQuery] SummaryPaginationQuery pagination)
        {
            var paginationFilter =
                new PaginationFilter 
                { 
                    PageSize = pagination.PageSize, 
                    PageNumber = pagination.PageNumber
                };

            var response = new SummaryPagedResponse<BatchFileSummaryDto>()
            {
                PageSize = paginationFilter.PageSize,
                PageNumber = paginationFilter.PageNumber
            };

            try
            {
                ValidateUserId(userId);

                var result = await _batchProcessor.GetUserFilesSummary(userId, paginationFilter);
                response.Data = result.Data;
                response.TotalCount = result.TotalRowsCount;
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured  {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                return BadRequest(new { errorMessage = "Unknown error occured. Please retry!." });
            }

            return Ok(response);
        }

        [HttpGet("download/{batchId}/statusresult")]
        public async Task<IActionResult> ValidationResultFile(string batchId)
        {
            try
            {
                var outputStream = new MemoryStream();

                var resultModel = await _batchProcessor.GetFileValidationResultAsync(batchId, outputStream);

                var contentType = MimeUtility.GetMimeMapping(resultModel.NasValidationFileName);

                outputStream.Seek(0, SeekOrigin.Begin);

                return File(outputStream, contentType, resultModel.RawFileName+'_'+GenericConstants.ValidationResultFile);
            }
            catch (AppException ex)
            {
                _logger.LogError("An Error occured {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var result = new ObjectResult(new { ex.Message })
                {
                    StatusCode = ex.StatusCode,
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("An Error occured {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                return BadRequest(new { errorMessage = "Unknown error occured. Please retry!." });
            }
        }
    }
}
