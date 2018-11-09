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
            services.AddMvc();
            services.AddSingleton<RequestBodyInitializer, RequestBodyInitializer>();
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

            app.UseMvc();

            TelemetryConfiguration.Active.TelemetryInitializers.Add(new RoleNameInitializer("PasApi"));
            TelemetryConfiguration.Active.TelemetryInitializers.Add(requestBodyInitializer);
        }
    }
}