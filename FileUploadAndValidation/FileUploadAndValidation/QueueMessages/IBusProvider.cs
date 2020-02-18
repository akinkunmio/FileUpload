using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueueServiceBus.BusProviders
{
    public interface IBusProvider
    {
       IBusControl CreateBus();
    }
}
