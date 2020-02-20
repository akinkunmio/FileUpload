using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
using FileUploadAndValidation.Repository;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using FileUploadAndValidation.QueueServices;
using Microsoft.Extensions.Hosting;

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

            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());

            services.AddSingleton<IAppConfig, AppConfig>();
            services.AddHttpClient<IBillPaymentService, BillPaymentService>();
            services.AddScoped<IBillPaymentDbRepository, BillPaymentRepository>();
            services.AddScoped<INasRepository, NasRepository>();
            services.AddScoped<IApiUploadService, ApiUploadService>();

            //services.AddScoped<FirsWhtFileService>();
            //services.AddScoped<AutoPayFileService>();
            //services.AddScoped<BulkSmsFileService>();
            services.AddScoped<BulkBillPaymentFileService>();

            services.AddTransient<Func<FileServiceTypeEnum, IFileService>>(serviceProvider => key => 
            {
                switch (key)
                {
                    //case FileServiceTypeEnum.FirsWht:
                    //    return serviceProvider.GetService<FirsWhtFileService>();
                    //case FileServiceTypeEnum.AutoPay:
                    //    return serviceProvider.GetService<AutoPayFileService>();
                    //case FileServiceTypeEnum.BulkSMS:
                    //    return serviceProvider.GetService<BulkSmsFileService>();
                    case FileServiceTypeEnum.BulkBillPayment:
                        return serviceProvider.GetService<BulkBillPaymentFileService>();
                    default:
                        return null;
                }
            });

            services.AddScoped<TxtFileReader>();
            services.AddScoped<CsvFileReader>();
            services.AddScoped<XlsFileReader>();
            services.AddScoped<XlsxFileReader>();
            
            services.AddTransient<Func<FileReaderTypeEnum, IFileReader>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case FileReaderTypeEnum.TXT:
                        return serviceProvider.GetService<TxtFileReader>();
                    case FileReaderTypeEnum.CSV:
                        return serviceProvider.GetService<CsvFileReader>();
                    case FileReaderTypeEnum.XLS:
                        return serviceProvider.GetService<XlsFileReader>();
                    case FileReaderTypeEnum.XLSX:
                        return serviceProvider.GetService<XlsxFileReader>();
                    default:
                        return null;
                }
            });

            services.AddHealthChecks();
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Info { Title = "My API", Version = "V1" });

                config.OperationFilter<FormFileSwaggerFilter>();
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2).AddViewComponentsAsServices();
            
            services.AddScoped<SendBillPaymentValidateMessageConsumer>();
            services.AddMassTransit(c =>
            {
                c.AddConsumer<SendBillPaymentValidateMessageConsumer>();
            });

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(@"{Configuration['AppSettings:RabbitMqUrl']}"), h =>
                {
                    h.Username(Configuration["AppSettings:QueueUserName"]);
                    h.Password(Configuration["AppSettings:QueuePassword"]);
                });

                cfg.ReceiveEndpoint(host, "qb-billpayments-latest", e =>
                {
                    e.PrefetchCount = 16;

                    e.LoadFrom(provider);
                    EndpointConvention.Map<SendBillPaymentValidateMessageConsumer>(e.InputAddress);
                });
            }));

            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IHostedService, BusService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.Extensions.Hosting.IHostingEnvironment env)
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

            app.UseHealthChecks(path: "/health");
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }

    }
}
