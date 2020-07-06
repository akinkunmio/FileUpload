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

namespace FileUploadApi.Controllers
{
    [Route("/qbupload/api/v1/[controller]")]
    [ApiController]
    public class AutoPayController : ControllerBase
    {
        private readonly IBatchFileProcessor<AutoPayUploadContext> _batchProcessor;
        private readonly ILogger<AutoPayController> _logger;
        private readonly IEnumerable<IFileReader> _fileReaders;

        public AutoPayController(IBatchFileProcessor<AutoPayUploadContext> batchProcessor,
                                 IEnumerable<IFileReader> fileReaders,
                                 ILogger<AutoPayController> logger)
        {
            _batchProcessor = batchProcessor;
            _logger = logger;
            _fileReaders = fileReaders;
        }

        [HttpPost("file")]
        public async Task<IActionResult> AutoPayUploadFile()
        {
            try
            {
                var uploadResult = new BatchFileSummary();
                var request = FileUploadRequest.FromRequestForSingle(Request);
                ValidateUserId(request.UserId.ToString());
                IFileReader fileContentReader = _fileReaders.FirstOrDefault(r => r.CanRead(request.FileExtension)) ?? throw new AppException("File extension not supported!.");

                using (var contentStream = request.FileRef.OpenReadStream())
                {
                    IEnumerable<Row> rows = fileContentReader.Read(contentStream);
                    var context = new AutoPayUploadContext();
                    uploadResult = await _batchProcessor.UploadAsync(rows, context);
                }
                
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

        private void ValidateUserId(string userId)
        {
        }
    }
}