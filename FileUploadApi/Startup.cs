using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;
using FileUploadAndValidation.FileReaders;
using FileUploadAndValidation.FileReaderImpl;
using FilleUploadCore.FileReaders;
using FileUploadApi.ApiServices;
using FileUploadAndValidation.UploadServices;
using FileUploadAndValidation.Utils;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.QueueServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using MassTransit;
using GreenPipes;
using DbUp;
using System.Reflection;

using AutoMapper;
using FileUploadAndValidation.Models;
using FileUploadAndValidation;

using FileUploadAndValidation.FileServices;
using FileUploadAndValidation.FileContentValidators;
using Hellang.Middleware.ProblemDetails;
using FileUploadApi.Services;
using FileUploadApi.Processors;
using FileUploadAndValidation.BillPayments;
using FileUploadApi.Utils;
using Microsoft.AspNetCore.Http;

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

            services.AddProblemDetails();

            services.AddScoped<IGenericUploadService, GenericUploadService>();
            services.AddTransient<ITokenHandlerRepository, TokenHandlerRepository>();
            services.AddTransient<ApprovalUtil>();
            services.AddSingleton<IAppConfig, AppConfig>();
            //services.AddHttpClient<IBillPaymentService, BillPaymentHttpService>();
            //services.AddScoped<IDbRepository<BillPayment, FailedBillPayment>, BillPaymentRepository>();
            services.AddScoped<INasRepository, NasRepository>();
            services.AddScoped<IBatchProcessor, BatchProcessor>();

            //services.AddScoped<IApiUploadService, ApiUploadService>();

            //services.AddScoped<FirsWhtFileService>();
            //services.AddScoped<AutoPayFileService>();
            //services.AddScoped<BulkSmsFileService>();
            //services.AddScoped<BillPaymentFileService>();

            #region File Content Remote Validators
            services.AddScoped<IFileContentValidator<AutoPayRow, AutoPayUploadContext>, AutoPayFileContentValidator>();
            services.AddScoped<IRemoteFileContentValidator<AutoPayRow>, AutoPayRemoteFileContentValidator>();

            services.AddScoped<IFileContentValidator<ManualCaptureRow, ManualCustomerCaptureContext>, ManualCaptureFileContentValidator>();
            services.AddScoped<IRemoteFileContentValidator<ManualCaptureRow>, ManualCaptureRemoteFileContentValidator>();

            services.AddScoped<IFileContentValidator<LASGPaymentRow, LASGPaymentContext>, LASGPaymentFileContentValidator>();
            services.AddScoped<IRemoteFileContentValidator<LASGPaymentRow>, LASGPaymentRemoteFileContentValidator>();
           #endregion

            services.AddScoped<BatchFileSummaryDbRepository>();

            services.AddScoped<IBatchProcessor, BatchProcessor>();
            services.AddScoped<IMultiTaxProcessor, MultiTaxProcessor>();
            services.AddScoped<ISingleTaxProcessor, SingleTaxProcessor>();

            //Setup File Readers per file extension
            services.AddScoped<IFileReader, TxtFileReader>();
            services.AddScoped<IFileReader, CsvFileReader>();
            services.AddScoped<IFileReader, XlsFileReader>();
            services.AddScoped<IFileReader, XlsxFileReader>();

            #region content firs content validators and batchrepo 
            services.AddScoped<IFileContentValidator, BillPaymentFileContentValidator>();
            services.AddScoped<IFileContentValidator, FirsFileContentValidator>();
            services.AddScoped<IFileContentValidator, FirsMultiTaxContentValidator>();
            services.AddScoped<IFileContentValidator, FctIrsMultiTaxContentValidator>();
            services.AddScoped<IFileContentValidator, FirsSingleTaxContentValidator>();

            services.AddScoped<IBatchRepository, BatchRepository>();
            services.AddScoped<IBatchRepository, MultiTaxBatchRepository>();
            services.AddScoped<IBatchRepository, SingleTaxBatchRepository>();
            #endregion

            services.AddScoped<IBatchFileProcessor<AutoPayUploadContext>, AutoPayBatchFileProcessor>();
            services.AddScoped<IBatchFileProcessor<ManualCustomerCaptureContext>, ManualCustomerCaptureBatchProcessor>();
            services.AddScoped<IBatchFileProcessor<LASGPaymentContext>, LASGPaymentBatchProcessor>();

            //Details tables repositories
            services.AddScoped<IDetailsDbRepository<AutoPayRow>, AutoPayDetailsDbRepository>();
            services.AddScoped<IDetailsDbRepository<ManualCaptureRow>, ManualCaptureDbRepository>();
            services.AddScoped<IDetailsDbRepository<LASGPaymentRow>, LasgPaymentDbRepository>();

            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IDbRepository, DbRepository>();

            services.AddHttpClient<IHttpService, HttpService>();

            services.AddScoped<INasRepository, NasRepository>();

            services.AddSingleton<IAppConfig, AppConfig>();
            
            services.AddHealthChecks();

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Info { Title = "QTB Upload Service API", Version = "V1" });

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };

                config.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                config.AddSecurityRequirement(security);

                config.OperationFilter<FormFileSwaggerFilter>();
            });
           
            services.AddCors();
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2).AddViewComponentsAsServices();
            
            services.AddScoped<SendBillPaymentValidateMessageConsumer>();
            services.AddMassTransit(c =>
            {
                c.AddConsumer<SendBillPaymentValidateMessageConsumer>();
            });

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(Configuration["AppConfig:RabbitMqUrl"]), h =>
                {
                    h.Username(Configuration["AppConfig:QueueUserName"]);
                    h.Password(Configuration["AppConfig:QueuePassword"]);
                });

                
                cfg.ReceiveEndpoint(host, Configuration["AppConfig:BillPaymentQueueName"], e =>
                {
                    e.PrefetchCount = 16;

                    e.UseMessageRetry(r => r.Interval(2, 100));

                    e.ConfigureConsumer<SendBillPaymentValidateMessageConsumer>(provider);

                });
            }));

            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IHostedService, BusService>();

            PerformScriptUpdate();
        }

        private void PerformScriptUpdate()
        {
            var connString = Configuration["ConnectionStrings:UploadServiceConnectionString"];
            var upgraderTran = DeployChanges.To
                .SqlDatabase(connString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
                    name => name.StartsWith("FileUploadApi.Scripts"))
                .LogToConsole()
                .Build();

            var resultEnt = upgraderTran.PerformUpgrade();
            if (!resultEnt.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(resultEnt.Error);
                Console.ResetColor();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.Extensions.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseProblemDetails();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(err => err.UseCustomErrors(env));
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseWhen(context => !PathExcludedFromAuthorization(context.Request.Path), appBuilder =>
            {
                appBuilder.UsePassportOauthMiddleware();
            });


            loggerFactory.AddSerilog();

            app.UseHealthChecks(path: "/health");
            app.UseHttpsRedirection();

            app.UseCors(options => options
               .AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               );

           // app.UseProblemDetails();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Upload Service V1.0");
                c.RoutePrefix = "api-docs";
            });
        }

        private bool PathExcludedFromAuthorization(PathString path)
        {
            if (path.StartsWithSegments("/health") 
                || path.StartsWithSegments("/api-docs") 
                || path.StartsWithSegments("/swagger")
                || path.StartsWithSegments("/qbupload/api/v1/"))
                return true;

            return false;
        }
    }
}
