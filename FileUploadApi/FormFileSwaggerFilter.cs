using FileUploadApi.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileUploadApi
{
    public class FormFileSwaggerFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var controller = context.MethodInfo.DeclaringType.FullName;
            var isLASGController = controller.EndsWith(nameof(LasgController));
            var isFCTIRSController = controller.EndsWith(nameof(FCTIrsController));
            var isAutoPayController = controller.EndsWith(nameof(AutoPayController));

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

            if (operation.OperationId == nameof(UploadController.TemplateUploadAsync))
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

            if (operation.OperationId == nameof(UploadController.PostMultiTaxPaymentUploadAsync))
            {
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "file",
                    In = "formData",
                    Description = "Upload File",
                    Required = false,
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

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "productCode",
                    In = "formData",
                    Required = true,
                    Type = "string"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "productName",
                    In = "formData",
                    Required = true,
                    Type = "string"
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "businessId",
                    In = "formData",
                    Required = true,
                    Type = "string"
                });

                operation.Consumes.Add("multipart/form-data");
                operation.Consumes.Add("string/form-data");
            }
           
            if(operation.OperationId == nameof(UploadController.ValidationResultFile) 
                || operation.OperationId == nameof(UploadController.GetTemplate))
            {
                operation.Produces = new[] { "application/octet-stream" };
            }

            if (operation.OperationId == nameof(LasgController.UploadFile) && isLASGController)
            {
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "userName",
                    In = "header",
                    Type = "string",
                    Required = false
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "file",
                    In = "formData",
                    Description = "Upload Lasg File",
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
                    Name = "businessId",
                    In = "formData",
                    Required = true,
                    Type = "string"
                });

                operation.Consumes.Add("string/header");
                operation.Consumes.Add("multipart/form-data");
                operation.Consumes.Add("string/form-data");
            }

            if (operation.OperationId == nameof(AutoPayController.UploadFile) && isAutoPayController)
            {
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "productCode",
                    In = "formData",
                    Type = "string",
                    Required = true
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "productName",
                    In = "formData",
                    Type = "string",
                    Required = true
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "businessTin",
                    In = "formData",
                    Type = "string",
                    Required = true
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "file",
                    In = "formData",
                    Description = "Upload Autopay File",
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
                    Name = "businessId",
                    In = "formData",
                    Required = true,
                    Type = "string"
                });

                operation.Consumes.Add("string/header");
                operation.Consumes.Add("multipart/form-data");
                operation.Consumes.Add("string/form-data");
            }
        
            if (operation.OperationId == nameof(FCTIrsController.UploadFile) && isFCTIRSController)
            {
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "userName",
                    In = "header",
                    Type = "string",
                    Required = false
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "file",
                    In = "formData",
                    Description = "Upload FCT IRS File",
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
                    Name = "businessId",
                    In = "formData",
                    Required = true,
                    Type = "string"
                });

                operation.Consumes.Add("string/header");
                operation.Consumes.Add("multipart/form-data");
                operation.Consumes.Add("string/form-data");

            }
        }        
    }
}