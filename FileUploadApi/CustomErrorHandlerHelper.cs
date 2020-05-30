﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FilleUploadCore.Exceptions;

namespace FileUploadApi
{
    public static class CustomErrorHandlerHelper
    {
        public static void UseCustomErrors(this IApplicationBuilder app, IHostingEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.Use(WriteDevelopmentResponse);
            }
            else
            {
                app.Use(WriteProductionResponse);
            }
        }

        private static Task WriteDevelopmentResponse(HttpContext httpContext, Func<Task> next)
            => WriteResponse(httpContext, includeDetails: true);

        private static Task WriteProductionResponse(HttpContext httpContext, Func<Task> next)
            => WriteResponse(httpContext, includeDetails: false);

        private static async Task WriteResponse(HttpContext httpContext, bool includeDetails)
        {
            // Try and retrieve the error from the ExceptionHandler middleware
            var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
            AppException ex = exceptionDetails?.Error as AppException;

            // Should always exist, but best to be safe!
            if (ex != null)
            {
                // ProblemDetails has it's own content type
                httpContext.Response.ContentType = "application/problem+json";

                // Get the details to display, depending on whether we want to expose the raw exception
                var title = /*includeDetails ? "An error occured: " + ex.Message : */"An error occured";
                var details = includeDetails ? ex.Message.ToString() : null;
                var instance = httpContext.Request.Path.ToString();

                var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = ex.StatusCode,
                    Title = title,
                   // Detail = details,
                    Instance = instance,
                };

                // This is often very handy information for tracing the specific request
                //var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
                //if (traceId != null)
                //{
                //    problem.Extensions["traceId"] = traceId;
                //}

                //Serialize the problem details object to the Response as JSON (using System.Text.Json)
                var stream = httpContext.Response.Body;
                await JsonSerializer.SerializeAsync(stream, problem);
            }
        }
    }
}
