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

namespace EnterpriseServerless.FunctionApp
{
    public static class LoaderIO
    {
        [FunctionName(nameof(LoaderIO))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string loaderIOVerificationToken = config["LoaderIO-VerificationToken"];
            string responseMessage = string.IsNullOrEmpty(loaderIOVerificationToken)
                ? "This HTTP triggered function executed successfully, however its missing the verification token."
                : loaderIOVerificationToken;

            return new OkObjectResult(responseMessage);
        }
    }
}
