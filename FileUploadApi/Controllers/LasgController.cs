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
    [Route("/qbupload/api/v1/billpayment/lasg")]
    [ApiController]
    public class LasgController : ControllerBase
    {
        private readonly IBatchFileProcessor<LASGPaymentContext> _batchProcessor;
        private readonly ILogger<LasgController> _logger;
        private readonly IEnumerable<IFileReader> _fileReaders;
        private readonly IAppConfig _appConfig;

        public LasgController(IBatchFileProcessor<LASGPaymentContext> batchProcessor,
                                         IEnumerable<IFileReader> fileReaders, IAppConfig appConfig,
                                         ILogger<LasgController> logger)
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
                // var productCode = "LASG";
                var request = FileUploadRequest.FromRequestForFCTIRS(Request);
                
                if (request.BusinessId == null || request.BusinessId < 1)
                    throw new AppException("Invalid BusinessId", "Invalid BusinessId");

                var context = new LASGPaymentContext(_appConfig)
                {
                    UserId = request.UserId.Value,
                    BusinessId = request.BusinessId.Value
                };

                IFileReader fileContentReader = _fileReaders.FirstOrDefault(r => r.CanRead(request.FileExtension)) ?? throw new AppException("File extension not supported!.");
                IEnumerable<Row> rows = new List<Row>();

                using (var contentStream = request.FileRef.OpenReadStream())
                {
                    var tempRows = fileContentReader.Read(contentStream);
                    rows = tempRows.Any() ? tempRows.Skip(1) : tempRows;
                }

                foreach (var row in rows)
                {
                    row.Columns[0].Value = context.ProductCode;
                }

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
    }
}