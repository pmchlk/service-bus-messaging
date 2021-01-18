using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryService.Infrastructure.Notifications.Subscribers.Handlers;
using InventoryService.Persistence.Database;
using Messaging.Meta;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Notifications.Extensions;
using Shared.Notifications.Models;
using Shared.Persistence.Database.Extensions;

namespace InventoryService
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
            services.AddSqlServerDbContext<InventoryDbContext>(Configuration, "ConnectionStrings:Default");
            services.AddServiceBusNotifications(Configuration);
            RegisterServiceBusDependencies(services);
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

            services.AddScoped<OrderIsInProgressEventHandler>();

            #endregion
        }

        private void ConfigureServiceBus(IApplicationBuilder applicationBuilder)
        {
            var eventBus = applicationBuilder.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderIsInProgressEvent, OrderIsInProgressEventHandler>();
        }
    }
}
