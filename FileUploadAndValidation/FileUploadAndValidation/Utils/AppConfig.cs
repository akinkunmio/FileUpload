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
        
        public string NasFolderLocation => _configuration["AppConfig:NasFolderLocation"];

        public string BillPaymentTransactionServiceBaseUrl => _configuration["AppConfig:BillPaymentTransactionServiceBaseUrl"];

        public string ValidateQueueUrl =>  _configuration["AppConfig:ValidateQueueUrl"];

        public string ValidateQueueName => _configuration["AppConfig:ValidateQueueName"];  

        public string QueuePassword => _configuration["AppConfig:QueuePassword"];

        public string QueueUsername => _configuration["AppConfig:QueueUsername"];
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
