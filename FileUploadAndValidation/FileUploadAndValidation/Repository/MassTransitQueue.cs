using FileUploadAndValidation.QueueMessages;
using FileUploadAndValidation.UploadServices;
using FileUploadAndValidation.Utils;
using FileUploadApi.Services;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Repository
{
    public class MassTransitQueue : IMassTransitQueue
    {
        private readonly IAppConfig _appConfig;
        private readonly IFileService _fileService;

        public MassTransitQueue(IAppConfig appConfig,
             Func<FileServiceTypeEnum, IFileService> fileService)
        {
            _appConfig = appConfig;
            _fileService = fileService(FileServiceTypeEnum.BulkBillPayment);
        }
        public async Task PublishMessage(BillPaymentValidateMessage validateMessage)
        {

            IBusControl bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri(_appConfig.RabbitMqUrl), h =>
                {
                    h.Username(_appConfig.QueueUsername);
                    h.Password(_appConfig.QueuePassword);
                });
            });

            await bus.StartAsync();

            await bus.Publish(validateMessage);

            await bus.StopAsync();
        }

        public async Task ConsumeMessage()
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri(_appConfig.RabbitMqUrl), h =>
                {
                    h.Username(_appConfig.QueueUsername);
                    h.Password(_appConfig.QueuePassword);
                });

                sbc.ReceiveEndpoint(_appConfig.BillPaymentQueueName, ep =>
                {
                    ep.Handler<BillPaymentValidateMessage>(async context =>
                    {
                        await _fileService.UpdateStatusFromQueue(context.Message);
                    });
                });

            });

            await bus.StartAsync();
        }
    }

    public interface IMassTransitQueue
    {
        Task PublishMessage(BillPaymentValidateMessage validateMessage);

        Task ConsumeMessage();
    }
   
}
