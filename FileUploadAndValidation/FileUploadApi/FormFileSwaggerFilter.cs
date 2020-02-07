using FileUploadApi.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileUploadApi
{
    public class FormFileSwaggerFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.OperationId == nameof(BillPaymentUploadController.PostBulkBillPaymentAsync))
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

        }
    }
}