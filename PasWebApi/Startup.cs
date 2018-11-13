using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace PasWebApi
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
            services
                .AddMvcCore()
                .AddJsonFormatters(settings => settings.NullValueHandling = NullValueHandling.Ignore);
            services.AddSingleton<RequestBodyInitializer, RequestBodyInitializer>();
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            RequestBodyInitializer requestBodyInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //  Configure CORS so that any apps can call the API
            //  Followed https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-2.1#enable-cors-with-cors-middleware
            app.UseCors(builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });

            app.UseMvc();

            TelemetryConfiguration.Active.TelemetryInitializers.Add(new RoleNameInitializer("PasApi"));
            TelemetryConfiguration.Active.TelemetryInitializers.Add(requestBodyInitializer);
        }
    }
}