using System;
using Messaging.AzureServiceBus.Meta;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Messaging.AzureServiceBus
{
    public class ServiceBusConnectionManager : IServiceBusConnectionManager
    {
        private readonly ILogger<ServiceBusConnectionManager> _logger;
        private readonly ServiceBusConnectionStringBuilder _connectionStringBuilder;
        private ITopicClient _topicClient;

        private bool _isDisposed;

        public ServiceBusConnectionManager(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder,
            ILogger<ServiceBusConnectionManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionStringBuilder = serviceBusConnectionStringBuilder ??
                                       throw new ArgumentNullException(nameof(serviceBusConnectionStringBuilder));
            _topicClient = new TopicClient(_connectionStringBuilder, RetryPolicy.Default);
        }

        public ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder => _connectionStringBuilder;

        public ITopicClient CreateTopicClient()
        {
            if(_topicClient.IsClosedOrClosing)
                _topicClient = new TopicClient(_connectionStringBuilder, RetryPolicy.Default);

            return _topicClient;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
        }
    }
}