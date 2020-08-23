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

namespace EnterpriseServerless.FunctionApp
{
    public class MediaFile
    {
        private readonly IMediaFileService _mediaFileService;

        public MediaFile(IMediaFileService mediaFileService)
        {
            _mediaFileService = mediaFileService;
        }

        [FunctionName(nameof(MediaFile))]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"HTTP trigger function processed a request for MediaFile, QueryString: {req.QueryString.Value}");

            return await _mediaFileService.GetMediaFileAsync(req.Query);
        }
    }
}
