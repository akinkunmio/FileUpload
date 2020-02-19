using FileUploadAndValidation.UploadServices;
using FileUploadAndValidation.Utils;
using FileUploadApi.Services;
using MassTransit;
using QueueServiceBus.BusProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueueServiceBus
{
    public class RabbitMqBusProvider : IBusProvider
    {
        private readonly IAppConfig _appConfig;

        public RabbitMqBusProvider(IAppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public IBusControl CreateBus()
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(configurator =>
            {
                var host = configurator.Host(new Uri(_appConfig.BillPaymentQueueUrl), h =>
                {
                    h.Username(_appConfig.QueueUsername);
                    h.Password(_appConfig.QueuePassword);
                });
            });

            bus.StartAsync();

            return bus;
        }
    }
}
