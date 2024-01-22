namespace UserAccounts.Api
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UserAccounts.Infrastructure;
    using UserAccounts.Service;

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
            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowedCorsOrigins",
                    builder =>
                    {
                        builder
                            .SetIsOriginAllowed(IsOriginAllowed)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });
            services.AddLogging();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "User Accounts",
                    Version = "v1",
                    Description = "User Management Service"
                });
            });

            services.AddInfrastructureServiceRegistrations();
            services.AddUserAccountsServiceRegistrations();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowedCorsOrigins");

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Accounts API v1");
                c.DefaultModelExpandDepth(2);
                c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });

            app.UseRouting();

           // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static bool IsOriginAllowed(string origin)
        {
            var uri = new Uri(origin);
            var env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "n/a";

            var isAllowed = uri.Host.Equals("example.com", StringComparison.OrdinalIgnoreCase)
                            || uri.Host.Equals("another-example.com", StringComparison.OrdinalIgnoreCase)
                            || uri.Host.EndsWith(".example.com", StringComparison.OrdinalIgnoreCase)
                            || uri.Host.EndsWith(".another-example.com", StringComparison.OrdinalIgnoreCase);
            if (!isAllowed && env.Contains("DEV", StringComparison.OrdinalIgnoreCase))
                isAllowed = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);

            return isAllowed;
        }
    }
}
