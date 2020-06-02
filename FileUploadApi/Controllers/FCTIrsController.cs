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

namespace FileUploadApi.Controllers
{    
    [Route("/qbupload/api/v1/billpayment/fct-irs")]
    [ApiController]
    public class FCTIrsController : ControllerBase
    {
        private readonly IBatchFileProcessor<ManualCustomerCaptureContext> _batchProcessor;
        private readonly ILogger<FCTIrsController> _logger;
        private readonly IEnumerable<IFileReader> _fileReaders;

        public FCTIrsController(IBatchFileProcessor<ManualCustomerCaptureContext> batchProcessor,
                                         IEnumerable<IFileReader> fileReaders,
                                         ILogger<FCTIrsController> logger)
        {
            _batchProcessor = batchProcessor;
            _logger = logger;
            _fileReaders = fileReaders;
        }


        [HttpPost("file")]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                var fctIRSProductCode = "FCT-IRS";
                var request = FileUploadRequest.FromRequestForSingle(Request);
                IEnumerable<Row> rows = new List<Row>();
                //ValidateUserId(request.UserId.ToString());
                IFileReader fileContentReader = _fileReaders.FirstOrDefault(r => r.CanRead(request.FileExtension)) ?? throw new AppException("File extension not supported!.");

                using (var contentStream = request.FileRef.OpenReadStream())
                {
                    rows = fileContentReader.Read(contentStream);
                }

                foreach (var row in rows)
                {
                    row.Columns[0].Value = fctIRSProductCode;
                }

                var context = new ManualCustomerCaptureContext
                {
                    Configuration = GetFCTIRSConfiguration()
                };

                var uploadResult = await _batchProcessor.UploadAsync(rows, context);

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