using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace EnterpriseServerless.FunctionApp
{
    public class LoaderIO
    {
        private static IConfigurationRoot _configuration { set; get; }
        private readonly IConfigurationRefresher _configurationRefresher;

        public LoaderIO(
            IConfigurationRoot configuration,
            IConfigurationRefresher configurationRefresher)
        {
            _configuration = configuration;
            _configurationRefresher = configurationRefresher;
        }

        [FunctionName(nameof(LoaderIO))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            await _configurationRefresher.TryRefreshAsync();

            string loaderIOVerificationToken = _configuration["EnterpriseServerless:LoaderIO_VerificationToken"];
            string responseMessage = string.IsNullOrEmpty(loaderIOVerificationToken)
                ? "This HTTP triggered function executed successfully, however its missing the verification token."
                : loaderIOVerificationToken;

            return new OkObjectResult(responseMessage);
        }
    }
}
