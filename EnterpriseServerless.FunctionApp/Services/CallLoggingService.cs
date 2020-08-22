using EnterpriseServerless.FunctionApp.Abstractions.Interfaces;
using EnterpriseServerless.FunctionApp.Abstractions.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static EnterpriseServerless.FunctionApp.Abstractions.Constants.Constants;

namespace EnterpriseServerless.FunctionApp.Services
{
    public class CallLoggingService : ICallLoggingService
    {
        private CosmosClient _cosmosClient;
        private readonly ILogger<CallLoggingService> _logger;
        private readonly IConfigurationRoot _configuration;
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public CallLoggingService(
            CosmosClient cosmosClient,
            ILogger<CallLoggingService> log,
            IConfigurationRoot configuration)
        {
            _cosmosClient = cosmosClient;
            _logger = log;
            _configuration = configuration;
        }

        public async Task CreateCallLogAsync(CallLog item)
        {
            _logger.LogInformation($"Creating CallLog for callSid: '{item.callSid}'");

            var container = _cosmosClient.GetContainer(CosmosDb.DatabaseId, CosmosDb.CallLogCollection);
            using (Stream stream = ToStream<CallLog>(item))
            {
                using (ResponseMessage responseMessage = await container.CreateItemStreamAsync(stream, new PartitionKey(item.callSid)))
                {
                    // Item stream operations do not throw exceptions for better performance
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        CallLog streamResponse = FromStream<CallLog>(responseMessage.Content);
                        _logger.LogInformation($"CallLog id: '{streamResponse.id}' created successfully");
#if DEBUG
                        _logger.LogDebug($"INSERT Diagnostics for CreateCallLogAsync: {responseMessage.Diagnostics}");
#endif
                    }
                    else
                    {
                        _logger.LogError($"Failed to create CallLog for callSid: '{item.id}' from stream. Status code: {responseMessage.StatusCode} Message: {responseMessage.ErrorMessage}");
                    }
                }
            }
        }

        public async Task UpdateCallLogAsync(string callSid)
        {
            _logger.LogInformation($"Updating CallLog for callSid: '{callSid}'");

            try
            {
                var container = _cosmosClient.GetContainer(CosmosDb.DatabaseId, CosmosDb.CallLogCollection);
                ItemResponse<CallLog> response = await container.ReadItemAsync<CallLog>(
                    id: callSid,
                    partitionKey: new PartitionKey(callSid));
#if DEBUG
                _logger.LogDebug($"READ Diagnostics for UpdateCallLogAsync for callSid: '{callSid}': {response.Diagnostics.ToString()}");
#endif

#if DEBUG
                if (response.Diagnostics != null)
                {
                    _logger.LogDebug($"READ Diagnostics for UpdateCallLogAsync for callSid: '{callSid}': {response.Diagnostics.ToString()}");
                }
#endif
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("CallLog details for '{0}' not found", callSid);
                    return;
                }

                var callLog = (CallLog)response;
                callLog.endTime = DateTime.UtcNow;
                callLog.lastUpdateDate = DateTime.UtcNow;

                // Save document
                response = await container.UpsertItemAsync(partitionKey: new PartitionKey(callLog.callSid), item: callLog);
                callLog = response.Resource;

                _logger.LogInformation($"Updated CallLog for callSid: '{callLog.callSid}'. StatusCode of this operation: {response.StatusCode}");
#if DEBUG
                _logger.LogDebug($"UPSERT Diagnostics for UpdateCallLogAsync for callSid: '{callSid}': {response.Diagnostics.ToString()}");
#endif
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"UpdateCallLogAsync error for callSid: '{callSid}'");
                throw;
            }
        }

        private static T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using (StreamReader sr = new StreamReader(stream))
                {
                    using (JsonTextReader jsonTextReader = new JsonTextReader(sr))
                    {
                        return Serializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }

        private static Stream ToStream<T>(T input)
        {
            MemoryStream streamPayload = new MemoryStream();
            using (StreamWriter streamWriter = new StreamWriter(streamPayload, encoding: Encoding.Default, bufferSize: 1024, leaveOpen: true))
            {
                using (JsonWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Newtonsoft.Json.Formatting.None;
                    Serializer.Serialize(writer, input);
                    writer.Flush();
                    streamWriter.Flush();
                }
            }

            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}
