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

        public string BillPaymentTransactionServiceUrl => _configuration["AppConfig:BillPaymentTransactionServiceUrl"];

        public string QueuePassword => _configuration["AppConfig:QueuePassword"];

        public string QueueUsername => _configuration["AppConfig:QueueUsername"];

        public string ProxyAddress => _configuration["AppConfig:ProxyAddress"];

        public string ProxyPort => _configuration["AppConfig:ProxyPort"];

        public string RabbitMqUrl => _configuration["AppConfig:RabbitMqUrl"];

        public string BillPaymentQueueName => _configuration["AppConfig:BillPaymentQueueName"];

    }

    public interface IAppConfig
    {
        string NasFolderLocation { get; }
        string BillPaymentTransactionServiceUrl { get; }
        string BillPaymentQueueName { get; }
        string QueuePassword { get; }
        string QueueUsername { get; }
        string UploadServiceConnectionString { get; }
        string ProxyAddress { get; }
        string ProxyPort { get; }
        string RabbitMqUrl { get; }
    }
}
