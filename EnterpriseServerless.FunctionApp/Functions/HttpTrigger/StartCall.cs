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
using System.Net.Mime;

namespace EnterpriseServerless.FunctionApp
{
    public class StartCall
    {
        private readonly IStartCallService _startCallService;

        public StartCall(IStartCallService startCallService)
        {
            _startCallService = startCallService;
        }

        [FunctionName(nameof(StartCall))]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = string.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            log.LogInformation($"HTTP trigger function processed a request for StartCall: {requestBody}");

            _startCallService.SetHostUrl(req.Host.Value, req.IsHttps);
            var result = await _startCallService.GetNumberDetailsAsync(requestBody);

            return new ContentResult
            {
                Content = result,
                ContentType = MediaTypeNames.Application.Xml,
                StatusCode = 200
            };
        }
    }
}
