using EnterpriseServerless.FunctionApp.Abstractions.Constants;
using EnterpriseServerless.FunctionApp.Abstractions.Interfaces;
using EnterpriseServerless.FunctionApp.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(EnterpriseServerless.FunctionApp.Startup))]

namespace EnterpriseServerless.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        private static IConfigurationRoot Configuration { get; set; }
        public IConfigurationBuilder ConfigurationBuilder { get; set; }
        private static IConfigurationRefresher ConfigurationRefresher { set; get; }

        public Startup()
        {
            ConfigurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddEnvironmentVariables();

            Configuration = ConfigurationBuilder.Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var regionName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.RegionName);
            if (string.IsNullOrWhiteSpace(regionName))
            {
                // Default to EastUS2 if the environment variable is missing/empty.
                regionName = Regions.EastUS2;
            }

            // Register the CosmosClient as a Singleton
            // Optimize for preferred geo-region 
            builder.Services.AddSingleton((s) =>
            {
                CosmosClientBuilder configurationBuilder = new CosmosClientBuilder(Configuration[Constants.CosmosDb.Connection])
                    .WithApplicationRegion(regionName);

                return configurationBuilder.Build();
            });

            // Load configuration from Azure App Configuration
            ConfigurationBuilder.AddAzureAppConfiguration(options =>
            {
                // Use ".Connect(...)" for connection string, or use ".ConnectWithManagedIdentity(...) for managed identity"
                options.Connect(Environment.GetEnvironmentVariable("AzureAppConfigConnectionString"))
                       // Load all keys that start with `EnterpriseServerless:`
                       .Select("EnterpriseServerless:*")
                       // Configure to reload configuration if the registered 'Sentinel' key is modified
                       .ConfigureRefresh(refreshOptions =>
                            refreshOptions.Register(key: "EnterpriseServerless:Sentinel", label: LabelFilter.Null, refreshAll: true)
                                          .SetCacheExpiration(TimeSpan.FromSeconds(120))
                       )
                       // Indicate to load feature flags
                       .UseFeatureFlags(flagOptions =>
                       {                           
                           flagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(120);
                       });
                ConfigurationRefresher = options.GetRefresher();
            });
            Configuration = ConfigurationBuilder.Build();

            // Singleton for the BlobServiceClient
            builder.Services.AddAzureClients(builder =>
            {
                var storageConnectionString = Environment.GetEnvironmentVariable(Constants.StorageAccount.ConnectionString);
                builder.AddBlobServiceClient(storageConnectionString)
                       .ConfigureOptions(options =>
                       {
                           options.GeoRedundantSecondaryUri = new Uri(Environment.GetEnvironmentVariable(Constants.StorageAccount.SecondaryConnectionUrl));
                           options.Retry.Mode = Azure.Core.RetryMode.Fixed;
                           options.Retry.MaxRetries = int.Parse(Environment.GetEnvironmentVariable(Constants.StorageAccount.GeoRedundantStorageMaxRetries));
                           options.Retry.Delay = TimeSpan.FromSeconds(double.Parse(Environment.GetEnvironmentVariable(Constants.StorageAccount.GeoRedundantStorageDelayInSeconds)));
                           options.Retry.MaxDelay = TimeSpan.FromSeconds(double.Parse(Environment.GetEnvironmentVariable(Constants.StorageAccount.GeoRedundantStorageMaxDelayInSeconds)));
                       });
            });

            builder.Services.AddLogging();
            builder.Services.AddSingleton(Configuration);
            builder.Services.AddSingleton(ConfigurationRefresher);
            builder.Services.AddFeatureManagement(Configuration);

            builder.Services.AddSingleton<IStartCallService, StartCallService>();
            builder.Services.AddSingleton<ICallLoggingService, CallLoggingService>();
            builder.Services.AddSingleton<IMediaFileService, MediaFileService>();
            builder.Services.AddSingleton<IPostCallService, PostCallService>();
        }
    }
}
