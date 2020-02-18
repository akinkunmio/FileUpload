using FileUploadAndValidation.QueueMessages;
using MassTransit;
using QueueServiceBus.BusProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueServiceBus.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private readonly IBusProvider _busProvider;

        public MessageBus(IBusProvider busProvider)
        {
            _busProvider = busProvider;
        }

        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancelationToken = default(CancellationToken))
            where TMessage : class, IMessage
        {
            return _busProvider.CreateBus().Publish(message, cancelationToken);
        }
    }
}
