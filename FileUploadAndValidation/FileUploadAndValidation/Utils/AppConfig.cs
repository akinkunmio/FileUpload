using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Utils
{
    public class AppConfig : IAppConfig
    {
        private readonly IConfiguration _configuration;

        public AppConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string UploadServiceConnectionString => _configuration["ConnectionStrings:UploadServiceConnectionString"];
        
        public string NasFolderLocation => _configuration["AppSettings:NasFolderLocation"];

        public string BillPaymentTransactionServiceBaseUrl => _configuration["AppSettings:BillPaymentTransactionServiceBaseUrl"];

        public string ValidateQueueUrl =>  _configuration["AppSettings:ValidateQueueUrl"];

        public string ValidateQueueName => _configuration["AppSettings:ValidateQueueName"];  

        public string QueuePassword => _configuration["AppSettings:QueuePassword"];

        public string QueueUsername => _configuration["AppSettings:QueueUsername"];
    }

    public interface IAppConfig
    {
        string NasFolderLocation { get; }
        string BillPaymentTransactionServiceBaseUrl { get; }
        string ValidateQueueUrl { get; }
        string ValidateQueueName { get; }
        string QueuePassword { get; }
        string QueueUsername { get; }
        string UploadServiceConnectionString { get; }
    }
}
