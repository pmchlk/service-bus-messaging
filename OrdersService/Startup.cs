using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messaging.Meta;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrdersService.Infrastructure.Notifications.Publishers.Dispatchers;
using OrdersService.Infrastructure.Notifications.Subscribers.Handlers;
using OrdersService.Persistence;
using Shared.Notifications.Extensions;
using Shared.Notifications.Models;
using Shared.Persistence.Database.Extensions;

namespace OrdersService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddOptions();
            services.AddSqlServerDbContext<OrderDbContext>(Configuration, "ConnectionStrings:Default");
            services.AddServiceBusNotifications(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            ConfigureServiceBus(app);
        }
        
        private void RegisterServiceBusDependencies(IServiceCollection services)
        {
            #region Handlers

            services.AddScoped<PaymentCompletedEventHandler>();

            #endregion
            #region Dispatchers

            services.AddScoped<OrderIsInProgressEventDispatcher>();

            #endregion
        }

        private void ConfigureServiceBus(IApplicationBuilder applicationBuilder)
        {
            var eventBus = applicationBuilder.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<PaymentCompletedEvent, PaymentCompletedEventHandler>();
        }
    }
}
