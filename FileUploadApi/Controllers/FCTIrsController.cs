using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using System;
using FilleUploadCore.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using FileUploadAndValidation;
using FileUploadApi.Processors;
using FileUploadAndValidation.BillPayments;
using FileUploadAndValidation.Utils;

namespace FileUploadApi.Controllers
{    
    [Route("/qbupload/api/v1/billpayment/fct-irs")]
    [ApiController]
    public class FCTIrsController : ControllerBase
    {
        private readonly IBatchFileProcessor<ManualCustomerCaptureContext> _batchProcessor;
        private readonly ILogger<FCTIrsController> _logger;
        private readonly IEnumerable<IFileReader> _fileReaders;
        private readonly IAppConfig _appConfig;

        public FCTIrsController(IBatchFileProcessor<ManualCustomerCaptureContext> batchProcessor,
                                         IEnumerable<IFileReader> fileReaders, IAppConfig appConfig,
                                         ILogger<FCTIrsController> logger)
        {
            _batchProcessor = batchProcessor;
            _logger = logger;
            _fileReaders = fileReaders;
            _appConfig = appConfig;
        }


        [HttpPost("file")]
        public async Task<IActionResult> FCTIrsUploadFile()
        {
            try
            {
                var productCode = _appConfig.FCTIRSProductCode;
                var request = FileUploadRequest.FromRequestForFCTIRS(Request);
                request.ProductCode = productCode;
                request.ProductName = productCode;

                IEnumerable<Row> rows = new List<Row>();
                IFileReader fileContentReader = _fileReaders.FirstOrDefault(r => r.CanRead(request.FileExtension)) ?? throw new AppException("File extension not supported!.");

                using (var contentStream = request.FileRef.OpenReadStream())
                {
                    var tempRows = fileContentReader.Read(contentStream);
                    rows = tempRows.Any() ? tempRows.Skip(1) : tempRows;
                }

                foreach (var row in rows)
                {
                    row.Columns[0].Value = productCode;
                }

                var context = new ManualCustomerCaptureContext
                {
                    Configuration = GetFCTIRSConfiguration(),
                    UserId = request.UserId ?? 0
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

        private ManualCaptureRowConfig GetFCTIRSConfiguration()
        {
            return new ManualCaptureRowConfig
            {
                IsPhoneNumberRequired = true
            };
        }
    }
}