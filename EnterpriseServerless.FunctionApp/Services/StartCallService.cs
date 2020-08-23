using EnterpriseServerless.FunctionApp.Abstractions.Enums;
using EnterpriseServerless.FunctionApp.Abstractions.Interfaces;
using EnterpriseServerless.FunctionApp.Abstractions.Models;
using EnterpriseServerless.FunctionApp.Helpers;
using EnterpriseServerless.FunctionApp.RequestModels;
using EnterpriseServerless.FunctionApp.ResponseModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
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

        private string _mediaFileUrl;
        private string _postCallUrl;

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
        }

        public void SetHostUrl(string requestHost, bool isHttps)
        {
            if (string.IsNullOrWhiteSpace(requestHost))
            {
                throw new Exception("Missing request host value.");
            }

            var hostUrl = isHttps == true
                ? $"https://{requestHost}"
                : $"http://{requestHost}";

            _mediaFileUrl = hostUrl;
            _postCallUrl = hostUrl;
        }

        public async Task<string> GetNumberDetailsAsync(string requestBody)
        {
            ValidateHostUrls();
            Response responseResult;
            try
            {                
                IncomingCall call = ParseCallParameters(requestBody);
                if (call == null)
                {
                    _logger.LogWarning($"Returning Twilio Disconnect Call response. Incoming call is null.");
                    responseResult = CallResponseService.DisconnectCallResponse();
                }
                else
                {
                    responseResult = await ProcessNumberRoutingAsync(call);
                }

                var resultJson = JsonConvert.SerializeObject(responseResult);
                _logger.LogInformation($"GetNumberDetailsAsync Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetNumberDetailsAsync Exception. Details: {ex.Message}");
                responseResult = CallResponseService.DisconnectCallResponse();
            }

            _logger.LogDebug($"{JsonConvert.SerializeObject(responseResult)}");

            var result = TwilioXmlSerializer.XmlSerialize(responseResult);
            _logger.LogInformation($"GetNumberRouteAsync response: {result}");

            return result;
        }

        private void ValidateHostUrls()
        {
            if (string.IsNullOrWhiteSpace(_mediaFileUrl))
            {
                throw new Exception("Missing request host value 'MediaFileUrl'");
            }

            if (string.IsNullOrWhiteSpace(_postCallUrl))
            {
                throw new Exception("Missing request host value 'PostCallUrl'");
            }
        }

        private async Task<Response> ProcessNumberRoutingAsync(IncomingCall call)
        {
            if (string.IsNullOrEmpty(call?.To))
            {
                _logger.LogWarning($"Returning Twilio Reject response for callSid: '{call.CallSid}'");
                return CallResponseService.RejectResponse();
            }

            try
            {
                var numberRouteInfo = await GetNumberRouteDataAsync(call.To);
                if (numberRouteInfo == null)
                {
                    _logger.LogInformation($"Returning Twilio Hangup response for callSid: '{call.CallSid}', StatusId is 'Error'");
                    await LogCallAsync(call, CallStatus.NotAssigned);
                    return CallResponseService.HangupResponse();
                }

                await LogCallAsync(call, CallStatus.Accepted);
                return HandleBasicRoute(numberRouteInfo.routeDetails.routes[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ProcessNumberRoutingAsync Exception. Details: {ex.Message}");
                await LogCallAsync(call, CallStatus.Error);
                throw;
            }
        }

        private async Task<NumberDetails> GetNumberRouteDataAsync(string number)
        {
            try
            {
                var container = _cosmosClient.GetContainer(CosmosDb.DatabaseId, CosmosDb.NumberRoutesCollection);

                _logger.LogInformation("Looking up routing details for number '{0}'", number);

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

        private IncomingCall ParseCallParameters(string data)
        {
            string[] items = data.Split('&');
            if (items.Length == 0)
            {
                _logger.LogError("StartCallService Exception: Body missing data parameters");
                return null;
            }

            var ret = new IncomingCall
            {
                Language = "en-US"
            };

            int found = 0;
            foreach (string item in items)
            {
                if (item.StartsWith("CallSid="))
                {
                    ret.CallSid = string.Compare(item.Substring(8), "LOADTEST", true) != 0 
                        ? item.Substring(8) 
                        : Guid.NewGuid().ToString();

                    found += 1;
                }
                else if (item.StartsWith("From="))
                {
                    ret.From = item.Substring(8);
                    found += 1;
                }
                else if (item.StartsWith("To="))
                {
                    ret.To = item.Substring(6);
                    if (ret.To.StartsWith("61"))
                    {
                        ret.Language = "en-AU";
                    }

                    found += 1;
                }

                if (found == 3)
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(ret.CallSid) || string.IsNullOrEmpty(ret.To))
            {
                return null;
            }

            return ret;
        }

        private async Task LogCallAsync(IncomingCall call, CallStatus status)
        {
            var callLogMessage = new CallLog(call.CallSid)
            {
                callerNumber = call.From,
                calledNumber = call.To,
                statusId = (int)status,
                statusName = status.ToString(),
                routeKey = string.Empty
            };

            await _callLoggingService.CreateCallLogAsync(callLogMessage);
        }

        private Response HandleBasicRoute(RouteDetails numInfo)
        {
            _logger.LogInformation($"HandleBasicRoute");

            var ret = new Response();
            var dial = ConstructDial(numInfo);

            if (numInfo.callRecordEnabled)
            {
                TwilioSay saying = null;
                TwilioPlay play = null;
                bool callRecordKeyPressRequired = (numInfo.callRecordConfirmationRequired ?? false) && (!string.IsNullOrEmpty(numInfo.callRecordNotificationText) || !string.IsNullOrEmpty(numInfo.callRecordNotificationFileId));

                if (!string.IsNullOrEmpty(numInfo.callRecordNotificationText))
                {
                    var lang = numInfo.terminationNumber.StartsWith("61")
                        ? "en-AU"
                        : "en-US";

                    saying = new TwilioSay { Loop = 1, Text = numInfo.callRecordNotificationText, Voice = "alice", Language = lang };
                }
                else if (!string.IsNullOrEmpty(numInfo.callRecordNotificationFileId))
                {
                    var fileLink = $"{numInfo.callRecordNotificationFileId}-{numInfo.callRecordNotificationFileName}";
                    var param = numInfo.callRecordNotificationFileType == 6
                        ? "mrsid"
                        : "mrid";
                    play = new TwilioPlay { Loop = 1, Text = $"{_mediaFileUrl}/api/mediafile?{param}={Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(fileLink))}&amp;tenantId=1234" };
                }
                ret.Saying = saying;
                ret.Play = play;
            }
            ret.Dialing = dial;

            return ret;
        }

        private TwilioDial ConstructDial(RouteDetails route)
        {
            var dial = new TwilioDial
            {
                TimeLimit = "60"
            };

            dial.Text = route.terminationNumber;
            dial.Action = $"{_postCallUrl}/api/postcall?cd={route.callServiceUrlParameter}";

            return dial;
        }
    }
}
