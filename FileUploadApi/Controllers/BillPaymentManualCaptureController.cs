﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploadAndValidation;
using FileUploadAndValidation.BillPayments;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Utils;
using FileUploadApi.Processors;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FileUploadApi.Controllers
{
    [Route("/qbupload/api/v1/billpayment/manualcapture")]
    [ApiController]
    public class BillPaymentManualCaptureController : ControllerBase
    {
        private readonly IBatchFileProcessor<ManualCustomerCaptureContext> _batchProcessor;
        private readonly ILogger<FCTIrsController> _logger;
        private readonly IEnumerable<IFileReader> _fileReaders;
        private readonly IAppConfig _appConfig;

        public BillPaymentManualCaptureController(IBatchFileProcessor<ManualCustomerCaptureContext> batchProcessor,
                                         IEnumerable<IFileReader> fileReaders, IAppConfig appConfig,
                                         ILogger<FCTIrsController> logger)
        {
            _batchProcessor = batchProcessor;
            _logger = logger;
            _fileReaders = fileReaders;
            _appConfig = appConfig;
        }

        [HttpPost("file")]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                var request = FileUploadRequest.FromRequestForManualCapture(Request);

                if (request.BusinessId == null || request.BusinessId < 1)
                    throw new AppException("Invalid BusinessId", "Invalid BusinessId");

                IEnumerable<Row> rows = new List<Row>();
                IFileReader fileContentReader = _fileReaders.FirstOrDefault(r => r.CanRead(request.FileExtension)) ?? throw new AppException("File extension not supported!.");

                using (var contentStream = request.FileRef.OpenReadStream())
                {
                    var tempRows = fileContentReader.Read(contentStream);
                    rows = tempRows.Any() ? tempRows.Skip(1) : tempRows;
                }

                foreach (var row in rows)
                {
                    row.Columns[0].Value = request.ProductCode;
                }

                var context = new ManualCustomerCaptureContext
                {
                    Configuration = GetManualCaptureConfiguration(),
                    UserId = request.UserId ?? 0,
                    BusinessId = request.BusinessId ?? 0,
                    ContentType = request.ContentType,
                    ProductCode = request.ProductCode,
                    ProductName = request.ProductName
                };

                var uploadResult = await _batchProcessor.UploadAsync(rows, context, HttpContext.Request.Headers["Authorization"]);
                uploadResult.UploadSuccessful = true;
                return Ok(uploadResult);
            }
            catch (AppException ex)
            {
                return Utils.ResponseHandler.HandleException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Utils.ResponseHandler.HandleException(ex);
            }
        }

        private ManualCaptureRowConfig GetManualCaptureConfiguration()
        {
            return new ManualCaptureRowConfig
            {
                IsPhoneNumberRequired = true,
                IsEmailRequired = true,
                IsAddressRequired = true,
                AutogenerateCustomerId = true
            };
        }
    }
}