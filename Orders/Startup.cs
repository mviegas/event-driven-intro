using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Orders.Infra;
using Orders.Orchestrator;
using Message = Common.Contracts.Message;

namespace Orders
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders", Version = "v1" });
            });

            services.AddDbContext<OrdersContext>(opt =>
                opt.UseNpgsql(Configuration.GetConnectionString("postgres")));

            services.AddCap(capOptions =>
            {
                capOptions.UseDashboard(d => d.PathMatch = "/cap");
                capOptions.UseRabbitMQ(cfg =>
                {
                    cfg.HostName = "127.0.0.1";
                    cfg.UserName = "guest";
                    cfg.Password = "guest";
                    cfg.Port = 5672;
                    cfg.ExchangeName = "marketplace";
                    cfg.VirtualHost = "/";
                });
                capOptions.UseEntityFramework<OrdersContext>().UsePostgreSql(cfg =>
                {
                    cfg.Schema = "messaging";
                });
            });

            services.RegisterConsumers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.MigrateDatabase();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
    
    public static class Extensions
    {
        public static void MigrateDatabase(this IApplicationBuilder builder)
        {
            var scopeFactory = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrdersContext>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

            try
            {
                context.Database.Migrate();
            }
            catch (Exception e)
            {
                loggerFactory.CreateLogger<Startup>().LogError(e, "Error while migrating database");
            }
        }
        
        
        public static void RegisterConsumers(this IServiceCollection services)
        {
            services.AddTransient<IOrchestrator, Orchestrator.Orchestrator>();
        }

        public static async Task PublishCorrelatedAsync<T>(this ICapPublisher bus, T message,
            string callbackName = "")
            where T : Message
        {
            var correlationId = Activity.Current?.Id ?? string.Empty;

            var customHeader = new Dictionary<string, string>();
            customHeader.Add(Headers.CallbackName, callbackName);
            customHeader.Add(Headers.CorrelationId, correlationId);

            message.CorrelationId = correlationId;

            await bus.PublishAsync(message.MessageType, message, customHeader);
        }
    }
}