using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace EnterpriseServerless.FunctionApp
{
    public static class Welcome
    {
        [FunctionName(nameof(Welcome))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var functionRuntimeVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
            var regionName = Environment.GetEnvironmentVariable("REGION_NAME");

            var responseMessage = new
            {
                Message = $"Welcome to the {appName}!",
                ApplicationName = appName,
                ApplicationVersion = appVersion,
                Region = regionName,
                FunctionRuntimeVersion = functionRuntimeVersion,
                CreatedDatetime = DateTime.UtcNow,
            };

            return new OkObjectResult(responseMessage);
        }
    }
}
