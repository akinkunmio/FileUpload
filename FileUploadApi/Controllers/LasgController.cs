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
    [Route("/qbupload/api/v1/billpayment/lasg")]
    [ApiController]
    public class LasgController : ControllerBase
    {
        private readonly IBatchFileProcessor<LASGPaymentContext> _batchProcessor;
        private readonly ILogger<LasgController> _logger;
        private readonly IEnumerable<IFileReader> _fileReaders;

        public LasgController(IBatchFileProcessor<LASGPaymentContext> batchProcessor,
                                         IEnumerable<IFileReader> fileReaders,
                                         ILogger<LasgController> logger)
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
                var fctIRSProductCode = "LASG";
                var request = FileUploadRequest.FromRequestForFCTIRS(Request);
                request.ProductCode = fctIRSProductCode;
                request.ProductName = fctIRSProductCode;

                IEnumerable<Row> rows = new List<Row>();
                IFileReader fileContentReader = _fileReaders.FirstOrDefault(r => r.CanRead(request.FileExtension)) ?? throw new AppException("File extension not supported!.");

                using (var contentStream = request.FileRef.OpenReadStream())
                {
                    rows = fileContentReader.Read(contentStream);
                }

                foreach (var row in rows)
                {
                    row.Columns[0].Value = fctIRSProductCode;
                }

                var context = new LASGPaymentContext();
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
    }
}