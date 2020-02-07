using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;
using FileUploadApi.Services;
using FileUploadAndValidation.FileReaders;
using FileUploadAndValidation.FileReaderImpl;
using FilleUploadCore.FileReaders;
using FileUploadApi.ApiServices;
using FileUploadAndValidation.UploadServices;
using FileUploadAndValidation.Utils;

namespace FileUploadApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.AddSingleton<IAppConfig, AppConfig>();
            services.AddScoped<IApiUploadService, ApiUploadService>();
            services.AddScoped<FirsWhtFileUploadService>();
            services.AddScoped<AutoPayFileService>();
            services.AddScoped<BulkSmsFileUploadService>();
            services.AddScoped<BulkBillPaymentFileUploadService>();

            services.AddTransient<Func<FileServiceTypeEnum, IFileService>>(serviceProvider => key => 
            {
                switch (key)
                {
                    case FileServiceTypeEnum.FirsWht:
                        return serviceProvider.GetService<FirsWhtFileUploadService>();
                    case FileServiceTypeEnum.AutoPay:
                        return serviceProvider.GetService<AutoPayFileService>();
                    case FileServiceTypeEnum.BulkSMS:
                        return serviceProvider.GetService<BulkSmsFileUploadService>();
                    case FileServiceTypeEnum.BulkBillPayment:
                        return serviceProvider.GetService<BulkBillPaymentFileUploadService>();
                    default:
                        return null;
                }
            });

            services.AddScoped<TxtCsvFileReader>();
            services.AddScoped<XlsFileReader>();
            services.AddScoped<XlsxFileReader>();
            
            services.AddTransient<Func<FileReaderTypeEnum, IFileReader>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case FileReaderTypeEnum.TXT_CSV:
                        return serviceProvider.GetService<TxtCsvFileReader>();
                    case FileReaderTypeEnum.XLS:
                        return serviceProvider.GetService<XlsFileReader>();
                    case FileReaderTypeEnum.XLSX:
                        return serviceProvider.GetService<XlsxFileReader>();
                    default:
                        return null;
                }
            });

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Info { Title = "My API", Version = "V1" });

                config.OperationFilter<FormFileSwaggerFilter>();
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
