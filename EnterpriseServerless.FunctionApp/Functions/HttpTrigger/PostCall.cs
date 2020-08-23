using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EnterpriseServerless.FunctionApp.Abstractions.Interfaces;
using EnterpriseServerless.FunctionApp.Abstractions.Constants;
using System.Net.Mime;

namespace EnterpriseServerless.FunctionApp
{
    public class PostCall
    {
        private readonly IPostCallService _postCallService;

        public PostCall(IPostCallService postCallService)
        {
            _postCallService = postCallService;
        }

        [FunctionName(nameof(PostCall))]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Queue("%ProcessCallQueueName%", Connection = Constants.StorageAccount.ConnectionString)] ICollector<string> postCallQueue,
            ILogger log)
        {
            log.LogInformation($"HTTP trigger function processed a request for PostCall, QueryString: {req.QueryString.Value}");

            try
            {
                string result = await _postCallService.ProcessPostCallAsync(req);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    log.LogInformation($"PostCall: Creating CloudQueueMessage for '{result}'");

                    postCallQueue.Add(result);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"PostCall: function exception for {req.QueryString.Value}");
            }

            return new ContentResult
            {
                Content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><Response></Response>",
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = 200
            };
        }
    }
}
