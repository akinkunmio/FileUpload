using System;
using FilleUploadCore.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FileUploadApi.Utils
{
    public static class ResponseHandler
    {
        public static IActionResult HandleException(Exception ex)
        {
            var errorMessage = "Unknown error occured. Please retry!.";
            var response = new ProblemDetails
            {
                Title = "Unknown error",
                Detail = errorMessage,
                Status = StatusCodes.Status500InternalServerError,
                //Type = exception.GetType()
            };

            if (ex is AppException)
            {
                response = HandleApplicationException(ex as AppException);
            }

            return new ObjectResult(response)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = response.Status
            };
        }

        private static ProblemDetails HandleApplicationException(AppException ex)
        {
            return new ProblemDetails
            {
                Title = ex.Message,
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                //Type = exception.GetType()
            };
        }
    }
}