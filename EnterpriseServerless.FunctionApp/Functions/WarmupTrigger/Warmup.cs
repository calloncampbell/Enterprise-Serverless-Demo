using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EnterpriseServerless.FunctionApp.Functions.WarmupTrigger
{
    public static class Warmup
    {
        [FunctionName(nameof(Warmup))]
        public static void Run(
            [WarmupTrigger()] WarmupContext context,
            ILogger log)
        {
            // Initialize shared dependencies here

            log.LogInformation("Function App instance is warm");
        }
    }
}
