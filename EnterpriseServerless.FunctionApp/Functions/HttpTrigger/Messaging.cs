using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EnterpriseServerless.FunctionApp
{
    public static class Messaging
    {
        [FunctionName(nameof(Messaging))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string requestBody = string.Empty;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }

                log.LogInformation($"HTTP trigger function processed a request for Messaging, body: {requestBody}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"HTTP trigger function processed a request for Messaging with an exception: {ex.Message}");
            }

            return new OkResult();
        }
    }
}
