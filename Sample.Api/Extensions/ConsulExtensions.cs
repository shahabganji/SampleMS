using System;
using System.Linq;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sample.Api.Configuration;

namespace Sample.Api.Extensions
{
    public static class ConsulExtensions
    {
        public static IApplicationBuilder RegisterWithConsul(
            this IApplicationBuilder app,
            IHostApplicationLifetime lifetime)
        {
            if (lifetime == null) throw new ArgumentNullException(nameof(lifetime));
            
            // Retrieve Consul client from DI
            var consulClient = app.ApplicationServices
                .GetRequiredService<IConsulClient>();
            var consulConfig = app.ApplicationServices
                .GetRequiredService<IOptions<ConsulConfig>>();
            // Setup logger
            var loggingFactory = app.ApplicationServices
                .GetRequiredService<ILoggerFactory>();
            var logger = loggingFactory.CreateLogger<IApplicationBuilder>();

            // Get server IP address
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.First();
            
            // Register service with consul
            var uri = new Uri(address);
            var registration = new AgentServiceRegistration()
            {
                ID = $"{consulConfig.Value.ServiceId}-{uri.Port}",
                Name = consulConfig.Value.ServiceName,
                Address = uri.Host,
                Port = uri.Port,
                Tags = new[] { "Students", "Courses", "School" }
            };

            logger.LogInformation("Registering with Consul");
            
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            consulClient.Agent.ServiceRegister(registration).Wait();

            // handle deregistration of the service upon application shutdown, graceful shutdown not killing
            lifetime.ApplicationStopping.Register(() => {
                logger.LogInformation("Deregistering from Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).Wait(); 
            });

            return app;
        }
    }
}
