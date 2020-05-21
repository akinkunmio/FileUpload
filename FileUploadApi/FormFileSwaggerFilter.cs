using FileUploadApi.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileUploadApi
{
    public class FormFileSwaggerFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.OperationId == nameof(UploadController.PostBulkUploadPaymentAsync))
            {
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "file",
                    In = "formData",
                    Description = "Upload File",
                    Required = true,
                    Type = "file"
                });

                operation.Consumes.Add("multipart/form-data");
            }
            if(operation.OperationId == nameof(UploadController.PostMultiTaxPaymentUploadAsync))
            {
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "file",
                    In = "formData",
                    Description = "Upload File",
                    Required = true,
                    Type = "file"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "id",
                    In = "formData",
                    Required = true,
                    Type = "string"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "hasHeaderRow",
                    In = "formData",
                    Required = true,
                    Type = "string"
                });

                operation.Consumes.Add("multipart/form-data");
                operation.Consumes.Add("string/form-data");
            }
        }
    }
}