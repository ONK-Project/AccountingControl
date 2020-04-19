using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingControl.Data;
using AccountingControl.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace AccountingControl
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
            services.Configure<AccountControlDBSettings>(
                Configuration.GetSection(nameof(AccountControlDBSettings)));

            services.AddSingleton<IAccountControlDBSettings>(sp =>
                sp.GetRequiredService<IOptions<AccountControlDBSettings>>().Value);

            services.AddSingleton<ISubmissionService, SubmissionService>();
            services.AddSingleton<IAccountConsumptionService, AccountConsumptionService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Account Control", Version = "v1" });
            });

            services.AddControllers()
                .AddNewtonsoftJson(options => options.UseMemberCasing()); ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Account Control v1");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
