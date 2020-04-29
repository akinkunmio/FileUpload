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

            //services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            //services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());

            services.AddSingleton<IAppConfig, AppConfig>();
            services.AddHttpClient<IBillPaymentService, BillPaymentHttpService>();
            services.AddScoped<IDbRepository<BillPayment, FailedBillPayment>, BillPaymentRepository>();
            services.AddScoped<IBatchRepository, BatchRepository>();
            services.AddScoped<INasRepository, NasRepository>();
            services.AddScoped<IBatchProcessor, BatchProcessor>();
            //services.AddScoped<IApiUploadService, ApiUploadService>();

            //services.AddScoped<FirsWhtFileService>();
            //services.AddScoped<AutoPayFileService>();
            //services.AddScoped<BulkSmsFileService>();
            services.AddScoped<BillPaymentFileService>();

            //Setup File Content Validators
            services.AddScoped<IFileContentValidator, BillPaymentFileService>();

            //Setup File Readers per file extension
            services.AddScoped<IFileReader, TxtFileReader>();
            services.AddScoped<IFileReader, CsvFileReader>();
            services.AddScoped<IFileReader, XlsFileReader>();
            services.AddScoped<IFileReader, XlsxFileReader>();

            services.AddAutoMapper(typeof(Startup));

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
            return;
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            loggerFactory.AddSerilog();

            app.UseHealthChecks(path: "/health");
            app.UseHttpsRedirection();

            app.UseCors(options => options
               .AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               );

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }

    }
}
