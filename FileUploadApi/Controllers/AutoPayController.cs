using FileUploadApi.ApiServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FileUploadAndValidation.Models;
using FileUploadApi.Models;
using FilleUploadCore.FileReaders;
using System;
using FilleUploadCore.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using FileUploadAndValidation;

namespace FileUploadApi.Controllers
{
    [Route("/qbupload/api/v1/[controller]")]
    [ApiController]
    public class AutoPayController : ControllerBase
    {
        private readonly IBatchFileProcessor<AutoPayUploadContext> _batchProcessor;
        private readonly ILogger<AutoPayController> _logger;
        private readonly IEnumerable<IFileReader> _fileReaders;

        public AutoPayController(IBatchFileProcessor<AutoPayUploadContext> batchProcessor, IEnumerable<IFileReader> fileReaders, ILogger<AutoPayController> logger)
        {
            // controller needs a reader
            // controller needs raw request

            _batchProcessor = batchProcessor;
            _logger = logger;
            _fileReaders = fileReaders;
        }

        [HttpPost("file")]
        public async Task<IActionResult> UploadFile(DateTime t)
        {
            var uploadResult = new BatchFileSummary();

            try
            {
                IFileUploadRequest request = new FileUploadRequest(Request);
                ValidateUserId(request.UserId.ToString());
                IFileReader fileContentReader = _fileReaders.FirstOrDefault(r => r.CanRead(request.FileExtension)) ?? throw new AppException("File extension not supported!.");

                using (var contentStream = request.FileRef.OpenReadStream())
                {
                    IEnumerable<Row> rows = fileContentReader.Read(contentStream);
                    var context = new AutoPayUploadContext();
                    uploadResult = await _batchProcessor.UploadAsync(rows, context);
                }
            }
            catch (AppException ex)
            {
                _logger.LogError($"Could not successfully conclude the Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);

                var problemDetails = new ProblemDetails
                {
                    Status = ex.StatusCode,
                    Type = $"https://httpstatuses.com/{ex.StatusCode}",
                    Title = ex.Message,
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path
                };

                return new ObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json" },
                    StatusCode = ex.StatusCode,
                };
           }
            catch (Exception ex)
            {                
                _logger.LogError("An Unexpected Error occured during Upload File Process: {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                return BadRequest(new { uploadResult, errorMessage = "Unknown error occured. Please retry!."});
            }

            return Ok(uploadResult);
        }

        private void ValidateUserId(string userId)
        {
        }
    }
}