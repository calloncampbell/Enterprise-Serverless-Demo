using Azure.Core;
using Azure.Storage.Blobs;
using EnterpriseServerless.FunctionApp.Abstractions.Constants;
using EnterpriseServerless.FunctionApp.Abstractions.Interfaces;
using EnterpriseServerless.FunctionApp.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseServerless.FunctionApp.Services
{
    public class MediaFileService : IMediaFileService
    {
        private readonly ILogger<MediaFileService> _logger;
        private readonly IConfigurationRoot _configuration;

        private const string FilePath = "filePath";
        private const string MediaType = "mediaType";
        private const string TenantId = "tenantId";

        public MediaFileService(
            ILogger<MediaFileService> log,
            IConfigurationRoot configuration)
        {
            _logger = log;
            _configuration = configuration;
        }

        public async Task<IActionResult> GetMediaFileAsync(IQueryCollection query)
        {
            var fileLink = query[query.Keys.Where(x => x.Contains(FilePath)).FirstOrDefault()];
            var mediaType = query[query.Keys.Where(x => x.Contains(MediaType)).FirstOrDefault()];
            var tenantId = query[query.Keys.Where(x => x.Contains(TenantId)).FirstOrDefault()];

            if (query.Count() == 0 
                || (string.IsNullOrEmpty(fileLink) && string.IsNullOrEmpty(mediaType) && string.IsNullOrEmpty(tenantId)))
            {
                _logger.LogWarning("GetMediaFileAsync - Missing one or more query string parameters");

                Response ret = new Response
                {
                    Saying = new TwilioSay { Text = string.Empty, Voice = "alice" }
                };

                return new OkObjectResult(ret);
            }

            try
            {
                fileLink = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(query[query.Keys.Where(x => x.Contains(FilePath)).FirstOrDefault()]));
                var relativeAddress = $"{tenantId}/{mediaType}";
                var fileName = $@"{fileLink.ToString().Substring(0, 2)}/{fileLink}";
                var cloudFile = $"{relativeAddress}/{fileName}";

                _logger.LogInformation($"Getting media file: '{cloudFile}' for organizationId: '{tenantId}' from storage account");

                // Optimize for Storage RS-GRS
                var options = new BlobClientOptions
                {
                    Diagnostics = { IsLoggingEnabled = true },
                    GeoRedundantSecondaryUri = new Uri(_configuration[Constants.StorageAccount.SecondaryConnectionUrl]),
                    Retry =
                    {
                        Mode = RetryMode.Exponential,
                        MaxRetries = int.Parse(_configuration[Constants.StorageAccount.GeoRedundantStorageMaxRetries] ?? "3"),
                        Delay = TimeSpan.FromSeconds(double.Parse(_configuration[Constants.StorageAccount.GeoRedundantStorageDelayInSeconds] ?? "0.1")),
                        MaxDelay = TimeSpan.FromSeconds(double.Parse(_configuration[Constants.StorageAccount.GeoRedundantStorageMaxDelayInSeconds] ?? "3"))
                    }
                };

                BlobClient blobClient = new BlobClient(_configuration[Constants.StorageAccount.ConnectionString], 
                    Constants.StorageAccount.TwilioMediaFilesBlobContainer, 
                    cloudFile, 
                    options);

                try
                {
                    MemoryStream memoryStream = new MemoryStream();
                    await blobClient.DownloadToAsync(memoryStream);
                    memoryStream.Position = 0;
                    return new FileStreamResult(memoryStream, contentType: "audio/wav");
                }
                catch (Azure.RequestFailedException ex)
                {
                    _logger.LogError(ex, $"RequestFailedException - accessing media file: '{cloudFile}' \n{ex.Message}");
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, $"General Exception - accessing media file: '{cloudFile}' \n{ex2.Message}");
                }

                throw new Exception("Unable to open file not streamed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetMediaFileAsync Exception. Details: {ex.Message}");

                Response ret = new Response
                {
                    Saying = new TwilioSay { Text = string.Empty, Voice = "alice" }
                };

                return new OkObjectResult(ret);
            }
        }
    }
}
