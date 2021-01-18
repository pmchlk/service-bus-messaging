using System;
using System.Text;
using System.Threading.Tasks;
using Messaging.AzureServiceBus.Meta;
using Messaging.Meta;
using Messaging.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Messaging.AzureServiceBus
{
    public class ServiceBusEventBus : IEventBus
    {
        private readonly IServiceBusConnectionManager _serviceBusConnectionManager;
        private readonly IEventBusSubscriptionManager _eventBusSubscriptionManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly SubscriptionClient _subscriptionClient;
        private readonly ILogger<ServiceBusEventBus> _logger;
        public ServiceBusEventBus(IServiceBusConnectionManager serviceBusConnectionManager,
            IEventBusSubscriptionManager eventBusSubscriptionManager,
            string subscriptionClientName,
            IServiceProvider serviceProvider,
            ILogger<ServiceBusEventBus> logger,
            int maxConcurrentCalls = 10)
        {
            if(string.IsNullOrEmpty(subscriptionClientName))
                throw new ArgumentNullException(nameof(subscriptionClientName));
            _serviceBusConnectionManager = serviceBusConnectionManager ?? throw new ArgumentNullException(nameof(serviceBusConnectionManager));
            _eventBusSubscriptionManager = eventBusSubscriptionManager ?? throw new ArgumentNullException(nameof(eventBusSubscriptionManager));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _subscriptionClient = new SubscriptionClient(_serviceBusConnectionManager.ServiceBusConnectionStringBuilder, subscriptionClientName);
            
            RegisterSubscriptionClientMessageHandler(maxConcurrentCalls);
            RemoveDefaultRule();
        }

        public void Subscribe<TEvent, TEventHandler>() where TEvent : Event where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name.Replace(nameof(Event), string.Empty);

            var alreadySubscribed = _eventBusSubscriptionManager.HasSubscriptionsForEvent<TEvent>();
            if (!alreadySubscribed)
            {
                try
                {
                    _subscriptionClient.AddRuleAsync(new RuleDescription
                    {
                        Filter = new CorrelationFilter(eventName),
                        Name = eventName,
                    }).GetAwaiter().GetResult();
                }
                catch (ServiceBusException)
                {
                    _logger.LogWarning($"Subscription to event '{eventName}' already exists'");
                }
            }
            _logger.LogInformation($"Subscribing to event '{eventName}' with '{typeof(TEventHandler).Name}'");
            _eventBusSubscriptionManager.AddSubscription<TEvent, TEventHandler>();
        }

        public void Unsubscribe<TEvent, TEventHandler>() where TEvent : Event where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name.Replace(nameof(Event), string.Empty);
            try
            {

            }
            catch (MessagingEntityNotFoundException)
            {
                _logger.LogWarning($"Subscription to event '{eventName} could not be found");
            }
            
            _logger.LogInformation($"Unsubscribing handler '{typeof(TEventHandler).Name}' 'from event {eventName}'");
            _eventBusSubscriptionManager.RemoveSubscription<TEvent, TEventHandler>();
        }
        
        private void RegisterSubscriptionClientMessageHandler(int maxConcurrentCalls)
        {
            _subscriptionClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var eventName = $"{message.Label}{nameof(Event)}";
                    var messageContent = Encoding.UTF8.GetString(message.Body);

                    if (await ProcessEvent(eventName, messageContent))
                        await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                }, new MessageHandlerOptions(HandlingEventExceptionHandler) { MaxConcurrentCalls = maxConcurrentCalls, AutoComplete = false });
        }

        private Task HandlingEventExceptionHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var exception = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogError(exception,
                $"Error occured during handling message: {exception.Message}. Context: {context}");

            return Task.CompletedTask;
        }
        private async Task<bool> ProcessEvent(string eventName, string message)
        {
            var processed = false;
            if (_eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = _eventBusSubscriptionManager.GetHandlersForEvent(eventName);
                foreach (var subscription in subscriptions)
                {
                    var handler = _serviceProvider.GetRequiredService(subscription.HandlerType);
                    if (handler == null) continue;
                    var eventType = _eventBusSubscriptionManager.GetEventType(eventName);
                    var @event = JsonConvert.DeserializeObject(message, eventType);
                    var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await (Task) handlerType.GetMethod("Handle")
                        .Invoke(handler, new object[] {@event});
                    processed = true;
                }
            }

            return processed;
        }

        private void RemoveDefaultRule()
        {
            try
            {
                _subscriptionClient
                    .RemoveRuleAsync(RuleDescription.DefaultRuleName)
                    .GetAwaiter()
                    .GetResult();
                
            }
            catch (MessagingEntityNotFoundException)
            {
                _logger.LogWarning($"Messaging entity '{RuleDescription.DefaultRuleName} could not be found'");
            }
        }

        public void Dispose()
        {
            _eventBusSubscriptionManager.Clear();
        }
    }
}