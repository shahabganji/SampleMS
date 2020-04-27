using System;
using Consul;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Api.Authorization;
using Sample.Api.Configuration;
using Sample.Api.Extensions;

// using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace Sample.Api
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

            services.AddHttpContextAccessor();
            
            AddAuthorization(services);

            services.Configure<ConsulConfig>(Configuration.GetSection("Consul:Config"));
            services.AddSingleton<IConsulClient, ConsulClient>(p =>
                new ConsulClient(consulConfig =>
                {
                    var address = this.Configuration["Consul:Address"];
                    consulConfig.Address = new Uri(address );
                }));
        }

        private static void AddAuthorization(IServiceCollection services)
        {
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "https://localhost:7001";
                    options.RequireHttpsMetadata = true;
                    options.Audience = "Api1";
                });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(nameof(MustSayHelloHeaderRequirement),
                    policy => policy.AddRequirements(new MustSayHelloHeaderRequirement()));
            });
            services.AddScoped<IAuthorizationHandler, MustSayHelloHeaderAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.RegisterWithConsul(lifetime);
        }
    }
}
