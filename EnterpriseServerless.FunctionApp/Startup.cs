using EnterpriseServerless.FunctionApp.Abstractions.Constants;
using EnterpriseServerless.FunctionApp.Abstractions.Interfaces;
using EnterpriseServerless.FunctionApp.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(EnterpriseServerless.FunctionApp.Startup))]

namespace EnterpriseServerless.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        private static IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddEnvironmentVariables()
            .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var regionName = Environment.GetEnvironmentVariable("REGION_NAME");
            if (string.IsNullOrWhiteSpace(regionName))
            {
                // Default to CanadaCentral if the environment variable is missing/empty.
                regionName = Regions.CanadaCentral;
            }

            // Register the CosmosClient as a Singleton
            builder.Services.AddSingleton((s) =>
            {
                CosmosClientBuilder configurationBuilder = new CosmosClientBuilder(configuration[Constants.CosmosDb.Connection])
                    .WithApplicationRegion(Regions.WestUS2);

                return configurationBuilder.Build();
            });

            builder.Services.AddLogging();
            builder.Services.AddSingleton(configuration);
            builder.Services.AddSingleton<IStartCallService, StartCallService>();
            builder.Services.AddSingleton<ICallLoggingService, CallLoggingService>();
            //builder.Services.AddSingleton<IInCallService, InCallService>();
            //builder.Services.AddSingleton<IPostCallService, PostCallService>();
        }
    }
}
