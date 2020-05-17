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
using KubeMQ.SDK.csharp;
using KubeMQ.SDK.csharp.Events;
using KubeMQ.SDK.csharp.Subscription;
using Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace AccountingControl
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment { get; set; }
        private IAccountConsumptionService accountConsumptionService;
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            CurrentEnvironment = env;
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{CurrentEnvironment.EnvironmentName}.json")
                .AddEnvironmentVariables()
                .Build();

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AccountControlDBSettings>(
                Configuration.GetSection(nameof(AccountControlDBSettings)));

            services.AddSingleton<IAccountControlDBSettings>(sp =>
                sp.GetRequiredService<IOptions<AccountControlDBSettings>>().Value);

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

            SetupEventStore();
        }

        private void SetupEventStore()
        {
            var ChannelName = Configuration["AccountControlKubeMQSettings:ChannelName"];
            var ClientID = Configuration["AccountControlKubeMQSettings:ClientID"];
            var KubeMQServerAddress = Configuration["AccountControlKubeMQSettings:KubeMQServerAddress"];

            accountConsumptionService = new AccountConsumptionService(new AccountControlDBSettings()
            {
                ConnectionString = Configuration["AccountControlDBSettings:ConnectionString"],
                DatabaseName = Configuration["AccountControlDBSettings:DatabaseName"],
                SubmissionCollectionName = Configuration["AccountControlDBSettings:SubmissionCollectionName"],
            });

            
            var subscriber = new Subscriber(KubeMQServerAddress);
            SubscribeRequest subscribeRequest = new SubscribeRequest()
            {
                Channel = ChannelName,
                ClientID = ClientID,
                EventsStoreType = EventsStoreType.StartAtSequence,
                EventsStoreTypeValue = 1,
                SubscribeType = SubscribeType.EventsStore
            };

            subscriber.SubscribeToEvents(subscribeRequest, HandleIncomingEvents, HandeIncomingErrors);
        }

        private void HandeIncomingErrors(Exception eventReceive)
        {
            Console.WriteLine(eventReceive.Message);
        }

        private void HandleIncomingEvents(EventReceive eventReceive)
        {
            if (eventReceive != null)
            {
                // Convert submission
                string strMsg = string.Empty;
                var obj = KubeMQ.SDK.csharp.Tools.Converter.FromByteArray(eventReceive.Body);
                Submission submission = (Submission)obj;

                // get account 
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(Configuration["AccountManagementURL"]);
                httpClient.DefaultRequestHeaders.Add("Accept", "text/json");

                var response = httpClient.GetAsync($"MeteringUnit/{submission.MeteringUnit.MeteringUnitId}").Result;

                var meteringUnit = JsonConvert.DeserializeObject<MeteringUnit>(response.Content.ReadAsStringAsync().Result);

                // Save accountConsumption
                accountConsumptionService.PostAccountConsumption(new AccountConsumption()
                {
                    Price = submission.SubmissionPrice,
                    RessourceUsage = submission.RessourceUsage,
                    Type = submission.MeteringUnit.Type,
                    UnitOfMeassure = submission.UnitOfMeassure,
                    Account = new Account() { AccountId = meteringUnit.AccountId },
                    TimeStamp = DateTime.Now, 
                    EventSequence = eventReceive.Sequence,
                });
            }
        }
    }
}
