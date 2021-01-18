using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messaging.Meta;
using Messaging.Models;
using Messaging.Settings;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Messaging.AzureServiceBus
{
    public abstract class ServiceBusEventDispatcher<TEvent> 
        : IDisposable, IAsyncDisposable, IEventDispatcher<TEvent> where TEvent : Event
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _topicName;
        
        protected readonly TopicClient TopicClient;
        
        protected ServiceBusEventDispatcher(string serviceBusConnectionString, string topicName)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
            _topicName = topicName;
            TopicClient = new TopicClient(_serviceBusConnectionString, _topicName);
        }

        protected ServiceBusEventDispatcher(TopicClient topicClient)
        {
            TopicClient = topicClient;
        }

        protected ServiceBusEventDispatcher(IOptions<ServiceBusSettings> serviceBusOptions)
        {
            var settings = serviceBusOptions.Value ?? throw new ArgumentNullException(nameof(serviceBusOptions));
            _serviceBusConnectionString = settings.ConnectionString;
            _topicName = settings.TopicName;
            TopicClient = new TopicClient(_serviceBusConnectionString, _topicName);
        }
        public void Publish(TEvent @event)
        {
            var message = PrepareMessage(@event);
            var topicClient = TopicClient;
            topicClient.SendAsync(message).GetAwaiter().GetResult();
        }

        public async Task PublishAsync(TEvent @event)
        {
            var message = PrepareMessage(@event);
            var topicClient = TopicClient;
            await TopicClient.SendAsync(message);
        }

        public async Task PublishManyAsync(ICollection<TEvent> @events)
        {
            var messages = @events.Select(PrepareMessage).ToList();
            await TopicClient.SendAsync(messages);
        }

        private Message PrepareMessage(Event @event)
        {
            var eventName = @event.GetType().Name.Replace(nameof(Event), string.Empty);
            var serializedMessage = JsonConvert.SerializeObject(@event);
            var messageBody = Encoding.UTF8.GetBytes(serializedMessage);

            return new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = messageBody,
                Label = eventName,
                ContentType = System.Net.Mime.MediaTypeNames.Application.Json
            };
        }

        public void Dispose()
        {
            TopicClient?.CloseAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            if (TopicClient != null)
                await TopicClient.CloseAsync();
        }
    }
}