using FileUploadAndValidation.QueueMessages;
using FileUploadAndValidation.UploadServices;
using FileUploadAndValidation.Utils;
using FileUploadApi.Services;
using MassTransit;
using QueueServiceBus.BusProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBus.BusProviders
{
    public class RabbitMqConsumerBusProvider
    {
        private readonly IAppConfig _appConfig;
        private readonly IFileService _bulkBillPaymentService;

        public RabbitMqConsumerBusProvider(IAppConfig appConfig, Func<FileServiceTypeEnum, IFileService> fileService)
        {
            _appConfig = appConfig;
            _bulkBillPaymentService = fileService(FileServiceTypeEnum.BulkBillPayment);
        }

        public void CreateBus()
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(configurator =>
            {
                var host = configurator.Host(new Uri(_appConfig.BillPaymentQueueUrl), h =>
                {
                    h.Username(_appConfig.QueueUsername);
                    h.Password(_appConfig.QueuePassword);
                });

                configurator.ReceiveEndpoint("qb-upload-validation", ep =>
                {
                    ep.Handler<BillPaymentValidateMessage>(async context =>
                    {
                        await _bulkBillPaymentService.UpdateStatusFromQueue(context.Message);

                    });
                });

            });

            bus.Start();
        }
    }
}
