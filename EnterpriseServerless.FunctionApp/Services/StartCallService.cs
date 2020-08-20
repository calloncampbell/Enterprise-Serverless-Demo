using EnterpriseServerless.FunctionApp.Abstractions.Interfaces;
using EnterpriseServerless.FunctionApp.Abstractions.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static EnterpriseServerless.FunctionApp.Abstractions.Constants.Constants;

namespace EnterpriseServerless.FunctionApp.Services
{
    public class StartCallService : IStartCallService
    {
        private CosmosClient _cosmosClient;
        private readonly ILogger<StartCallService> _logger;
        private readonly IConfigurationRoot _configuration;
        private readonly ICallLoggingService _callLoggingService;

        private readonly bool _useQueryOptimization;

        public StartCallService(
            CosmosClient cosmosClient,
            ILogger<StartCallService> log,
            IConfigurationRoot configuration,
            ICallLoggingService callLoggingService)
        {
            _cosmosClient = cosmosClient;
            _logger = log;
            _configuration = configuration;
            _callLoggingService = callLoggingService;

            _useQueryOptimization = bool.Parse(_configuration["UseQueryOptimization"] ?? "false");
        }

        public async Task<string> GetNumberDetailsAsync(string requestBody)
        {
            try
            {
                var numberDialed = "TODO";// ParseCallParameters(requestBody);
                NumberDetails responseResult;
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    _logger.LogError($"Request body is null.");
                    return null;                    
                }
                else
                {
                    responseResult = await GetNumberRouteDataAsync(numberDialed);
                }

                var resultJson = JsonConvert.SerializeObject(responseResult);
                _logger.LogInformation($"GetNumberDetailsAsync Success");

                return resultJson;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetNumberDetailsAsync Exception. Details: {ex.Message}");
                return null;
            }
        }

        private async Task<NumberDetails> GetNumberRouteDataAsync(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                return null;
            }

            _logger.LogInformation("Looking up routing details for number '{0}'", number);

            try
            {
                var container = _cosmosClient.GetContainer(CosmosDb.DatabaseId, CosmosDb.NumberDetailsCollection);

                // ItemResponse uses 1 RU
                if (_useQueryOptimization)
                {
                    // Read the item to see if it exists. Note ReadItemAsync will not throw an exception if an item does not exist. Instead, we check the StatusCode property off the response object.                     
                    ItemResponse<NumberDetails> response = await container.ReadItemAsync<NumberDetails>(number, new PartitionKey(number));
#if DEBUG
                    if (response.Diagnostics != null)
                    {
                        _logger.LogDebug($"GetRouteDataAsync Cosmos DB Diagnostics: {response.Diagnostics}");
                    }
#endif
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogDebug("Number routing details for '{0}' not found", number);
                        return null;
                    }

                    return response;
                }
                else // FeedIterator uses 2.83 RU
                {
                    // Cosmos DB queries are case sensitive.
                    var query = @"SELECT * FROM c WHERE c.Number = @number";

                    QueryDefinition queryDefinition = new QueryDefinition(query)
                        .WithParameter("@number", number);

                    List<NumberDetails> results = new List<NumberDetails>();
                    using (FeedIterator<NumberDetails> resultSetIterator = container.GetItemQueryIterator<NumberDetails>(queryDefinition))
                    {
                        while (resultSetIterator.HasMoreResults)
                        {
                            FeedResponse<NumberDetails> response = await resultSetIterator.ReadNextAsync();
                            results.AddRange(response);
#if DEBUG
                            if (response.Diagnostics != null)
                            {
                                _logger.LogDebug($"GetRouteDataAsync Cosmos DB Diagnostics: {response.Diagnostics}");
                            }
#endif
                        }
                    }

                    var result = results.Take(1).FirstOrDefault();
                    if (result == null)
                    {
                        return null;
                    }

                    return result;
                }
            }
            catch (CosmosException cex)
            {
                _logger.LogError($"GetNumberRouteDataAsync CosmosException for number '{number}'. Details: {cex}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetNumberRouteDataAsync Exception for number '{number}'. Details: {ex}");
                return null;
            }
        }
    }
}
