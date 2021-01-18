using System;
using Microsoft.Azure.ServiceBus;

namespace Messaging.AzureServiceBus.Meta
{
    public interface IServiceBusConnectionManager : IDisposable
    {
        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder { get; }
        ITopicClient CreateTopicClient();
    }
}