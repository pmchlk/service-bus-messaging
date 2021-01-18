using Messaging;
using Messaging.AzureServiceBus;
using Messaging.AzureServiceBus.Meta;
using Messaging.Meta;
using Messaging.Settings;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Notifications.Extensions
{
    public static class ServiceBusNotificationsExtensions
    {
        public static void AddServiceBusNotifications(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("ServiceBus:ConnectionString");
            var mainTopicName = configuration.GetValue<string>("ServiceBus:MainTopicName");
            services.Configure<ServiceBusSettings>(settings =>
            {
                settings.ConnectionString = connectionString;
                settings.TopicName = mainTopicName;
            });
            services.AddSingleton<IServiceBusConnectionManager>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ServiceBusConnectionManager>>();
                var serviceBusConnectionString = connectionString;
                var connectionStringBuilder = new ServiceBusConnectionStringBuilder(serviceBusConnectionString)
                {
                    EntityPath = mainTopicName
                };
                return new ServiceBusConnectionManager(connectionStringBuilder, logger);
            });
            RegisterEventBus(services, configuration);
        }

        private static void RegisterEventBus(IServiceCollection services, IConfiguration configuration)
        {
            var subscriptionClientName = configuration.GetValue<string>("ServiceBus:SubscriptionName");
            services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();
            services.AddSingleton<IEventBus, ServiceBusEventBus>(serviceProvider =>
            {
                var serviceBusConnectionManager = serviceProvider.GetRequiredService<IServiceBusConnectionManager>();
                var logger = serviceProvider.GetRequiredService<ILogger<ServiceBusEventBus>>();
                var subscriptionManager = serviceProvider.GetRequiredService<IEventBusSubscriptionManager>();
                return new ServiceBusEventBus(serviceBusConnectionManager, subscriptionManager, subscriptionClientName,
                    serviceProvider, logger);
            });
        }
    }
}